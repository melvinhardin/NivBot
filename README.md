# NivBot
Personal project for discord, osrs and other things

## Setup

1. Copy `NivBot/appsettings.example.json` to `NivBot/appsettings.json` and fill in the Discord token and connection string.
2. Start the database: `docker compose up -d`
3. Run the bot (from Visual Studio or `dotnet run --project NivBot`). Pending migrations are applied on startup.
4. In Discord, run `/syncactivities` once (and again after Jagex adds bosses), then `/syncitems`.

After changing anything in `NivBot.DataLayer`, add a migration:

```
dotnet ef migrations add <Name> --project NivBot
```

## Running in docker

Put the secrets in a `.env` file next to `compose.yaml` (gitignored):

```
DISCORD_TOKEN=...
POSTGRES_PASSWORD=...
```

Then `docker compose --profile bot up -d --build`. `POSTGRES_PASSWORD` only takes effect when the database volume is created; an existing volume keeps its password.

## Commands

| Command | Who | What |
|---|---|---|
| `/register` | everyone | Register with the bot |
| `/addcharacter` | everyone | Link an OSRS account (can link several) |
| `/skilltask`, `/bosstask` | everyone | Show the current task, get a new one when it's done. Buttons to skip (cooldown) or block (capped) |
| `/taskblocks` | everyone | Remove blocked tasks |
| `/syncactivities`, `/syncitems`, `/syncgroupitems` | admins | Sync activities, collection log items, group collection logs |

## Tasks

Task progress is summed over all linked accounts and read from the hiscores whenever a task command is used.
Goals are sized to `Tasks:MinHours`–`Tasks:MaxHours` using the rates in the `SkillRates` and `BossRates` tables,
seeded from [Wise Old Man](https://github.com/wise-old-man/wise-old-man)'s efficiency rates and scaled by
`Tasks:SkillRateMultiplier` / `Tasks:BossRateMultiplier`. Tune rates in pgAdmin; a skill or boss without a rate is never assigned.
Bosses nobody should get (e.g. Mimic, Hespori) go in the `GlobalActivityBlockLists` table.
