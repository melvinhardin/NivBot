# NivBot
Personal project for discord, osrs and other things

## Setup

1. Copy `NivBot/appsettings.example.json` to `NivBot/appsettings.json` and fill in the Discord token and connection string
   (and the Temple / Wise Old Man group settings if you use them).
2. Start the database: `docker compose up -d`
3. Run the bot (from Visual Studio or `dotnet run --project NivBot`). Pending migrations are applied on startup.
4. In Discord, run `/syncactivities` once (and again after Jagex adds bosses), then `/syncitems`.
5. Give the bot the **Manage Roles** permission and drag its role above every role it should hand out.

After changing anything in `NivBot.DataLayer`, add a migration:

```
dotnet ef migrations add <Name> --project NivBot
```

## Running in docker

Put the settings in a `.env` file next to `compose.yaml` (gitignored):

```
DISCORD_TOKEN=...
POSTGRES_PASSWORD=...
TEMPLE_GROUP_ID=...
TEMPLE_GROUP_KEY=...
WOM_GROUP_ID=...
WOM_GROUP_VERIFICATION_CODE=...
ROLES_ANNOUNCEMENT_CHANNEL_ID=...
```

Then `docker compose --profile bot up -d --build`. `POSTGRES_PASSWORD` only takes effect when the database volume is created; an existing volume keeps its password.

## Commands

| Command | Who | What |
|---|---|---|
| `/register` | everyone | Register with the bot |
| `/addcharacter` | everyone | Link an OSRS account (can link several) |
| `/skilltask`, `/bosstask` | everyone | Show the current task, get a new one when it's done. Buttons to skip (cooldown) or block (capped) |
| `/taskblocks` | everyone | Remove blocked tasks |
| `/synccollection` | everyone | Update your collection log from TempleOSRS (points for new items) |
| `/points`, `/leaderboard` | everyone | Your points / the top 10 |
| `/updateroles` | everyone | Check your achievements and update your roles right away |
| `/syncactivities`, `/syncitems`, `/syncgroupitems` | admins | Sync activities, collection log items, group collection logs |
| `/setitempoints`, `/setbosspoints` | admins | Points per collection log item / bonus points for a boss task of that boss |
| `/roles add`, `remove`, `list`, `import`, `sync` | admins | Manage the roles the bot hands out |
| `/competition create`, `list`, `delete` | admins | Skill / boss competitions on Wise Old Man or TempleOSRS |

## Tasks

Task progress is summed over all linked accounts and read from the hiscores whenever a task command is used.
Goals are sized to `Tasks:MinHours`–`Tasks:MaxHours` using the rates in the `SkillRates` and `Bosses` tables,
seeded from [Wise Old Man](https://github.com/wise-old-man/wise-old-man)'s efficiency rates and scaled by
`Tasks:SkillRateMultiplier` / `Tasks:BossRateMultiplier`. Tune rates in pgAdmin; a skill or boss without a rate is never assigned.
Bosses nobody should get (e.g. Mimic, Hespori) go in the `GlobalActivityBlockLists` table.
Completing a boss task pays `Tasks:PointsPerTask` plus the boss's own points (`/setbosspoints`).

## Roles

Each role rule is a metric, a threshold and a Discord role:

| Metric | Measures | Over linked accounts |
|---|---|---|
| `ActivityScore` | KC / score of one boss or activity (e.g. Vorkath, Clue Scrolls (all)) | added up |
| `TotalBossKc` | KC of all bosses in the `Bosses` table | added up |
| `RaidsCompleted` | CoX, ToB and ToA (all modes) | added up |
| `CollectionLogItem` | an item in the TempleOSRS collection log (e.g. Fire cape) | added up |
| `TotalLevel`, `TotalXp` | hiscores overall | best account |
| `SkillsAt99`, `SkillsAt200m` | number of skills at 99 / 200m XP | best account |
| `SkillLevel` | level in one skill | best account |

- Roles with the same **tier group** are tiers of one badge: a member keeps only the highest tier, lower tiers are removed.
  Otherwise the bot never removes roles, and never touches roles without a rule (manual badges like CA tiers or the quest cape).
- With a **close trigger**, the bot posts once in `Roles:AnnouncementChannelId` when a member passes it.
- Every `Roles:SyncInterval` the bot syncs the Temple group collection logs (if `Temple:GroupId` is set) and checks every member.

`roles-import.csv` holds the boss Expert roles from the old bot; import it with `/roles import`. Role ids that don't exist
in the server are rejected, so check the report. `roles-badges-template.csv` holds the badges: create the roles in Discord,
fill in their ids (Developer Mode → right click the role → Copy Role ID) and import it too.

## Competitions

`/competition create` creates a competition for your Wise Old Man or TempleOSRS group (set the group settings first).
Shortly before a WOM competition ends the bot asks WOM to update everyone. `Competitions:ResultsDelay` after the end it posts
the top places in the channel where the competition was created and gives them `Competitions:Rewards` points; winners who
aren't linked to the bot get no points.
