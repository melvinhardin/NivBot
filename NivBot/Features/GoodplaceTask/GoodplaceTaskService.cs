using Microsoft.EntityFrameworkCore;
using NetCord;
using NivBot.DataLayer;
using NivBot.DataLayer.Enums;
using NivBot.DataLayer.Models;
using NivBot.ExternalServicesLayer.OsrsAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace NivBot.Features.GoodplaceTask
{
    public class GoodplaceTaskService(GoodplaceContext db, IOsrsHighscoreService osrsHighscore)
    {
        public async Task<GoodplaceTaskResult> GetGoodplaceSkillTask(long discordId)
        {
            // Get All available skill tasks (all skills)
            var availableTasks = Enum
                .GetValues<Skills>()
                .Cast<Skills>()
                .ToList();

            // Global skill blocklist
            List<Skills> globalBlockList = new List<Skills> { Skills.Attack, Skills.Defence, Skills.Strength, Skills.Magic, Skills.Hitpoints, Skills.Ranged, Skills.Prayer };


            // Get the user blocklist
            var userBlockList = await db.SkillTaskBlockLists
                .Where(x => x.GoodplaceUser.DiscordUserId == discordId)
                .Select(x => x.Skill)
                .ToListAsync();
            
            // See if the user has runescape accounts
            // Collect skill data on the all of the users runescape accounts
            var xpList = await db.RunescapeStats
                .Where(x => x.RunescapeAccount.GoodplaceUser.DiscordUserId == discordId)
                .ToListAsync();
            
            // Exit if the xpList is empty (no account)
            if (xpList.Count == 0) { return GoodplaceTaskResult.FailureNotImplemented; }

            // Get the current task (if it exists)
            var currentTask = await db.GoodplaceSkillTasks
                .Where(x => x.GoodplaceUser.DiscordUserId == discordId)
                .FirstOrDefaultAsync();

            var userWallet = await db.Wallets
                .Where(x => x.GoodplaceUser.DiscordUserId == discordId)
                .FirstAsync();

            // Prepare variables.
            Skills newTask;
            bool awardPoints = false;
            
            // Check if this is the users first task
            if (currentTask == null)
            {
                // A very silly workaround for the first task of the user
                // We set the current task to one in the global blocklist, the method checks for this
                // Not very efficient, but this is what I came up with, with my current knowledge
                newTask = GetRandomTask<Skills>(availableTasks, globalBlockList, userBlockList, Skills.Attack);
                GoodplaceSkillTask newSkillTask = new GoodplaceSkillTask
                {
                    GoodplaceUserId = userWallet.GoodplaceUserId,
                    SummedCurrentXp = xpList
                    .Where(x => x.Skill == newTask)
                    .Select(x => x.Xp)
                    .Aggregate(0L, (a, b) => a + b),
                    // TODO add a method to generate a goal based on xph
                    GoalXp = 1,
                    Skill = newTask
                };
                db.GoodplaceSkillTasks.Add(newSkillTask);
            }
            else
            {
                // Check if the user has completed the existing Task and award points

                
                // Compare the aggregated data against the goal xp
                if (xpList
                    .Where(x => x.Skill == currentTask.Skill)
                    .Select(x => x.Xp)
                    .Aggregate(0L, (total, b) => total + b) < currentTask.GoalXp)
                {
                    // Exit if xp is lower than the goal amount (task not completed)
                    return GoodplaceTaskResult.FailureNotImplemented;
                }
                newTask = GetRandomTask<Skills>(availableTasks, globalBlockList, userBlockList, currentTask.Skill);
                awardPoints = true;

                // Change the task to the new one
                currentTask.SummedCurrentXp = xpList
                    .Where(x => x.Skill == newTask)
                    .Select(x => x.Xp)
                    .Aggregate(0L, (a, b) => a + b);
                // TODO add a method to generate a goal based on xph
                currentTask.GoalXp = 1;
                currentTask.Skill = newTask;
            }

            // Award points if a task was completed
            if (awardPoints) 
            {
                userWallet.GoodplacePoints += 5;
                userWallet.GoodplaceCurrency += 5;
            }
            await db.SaveChangesAsync();

            return GoodplaceTaskResult.Success;
        }

        // A Helper method to return a random task, generic so that it works for skill or activity tasks.
        static private T GetRandomTask<T>(List<T> availableTasks, List<T> globalBlocklist, List<T> userBlocklist, T currentTask)
        {
            // For first task the current task will always be in the global blocklist, if not remove the currenttask
            if (!globalBlocklist.Contains(currentTask)) { availableTasks.Remove(currentTask); }

            // Removing all tasks in the blocklists
            availableTasks = availableTasks.Except(globalBlocklist).Except(userBlocklist).ToList();

            // New random seed and return a random task
            Random rnd = new Random();
            return availableTasks[rnd.Next(availableTasks.Count)];
        }

        public async Task<GoodplaceTaskResult> GetGoodplaceBossTask(long discordId)
        {
            // Load all of the tables needed. Activities, wallet, user & global blocklist, current task
            var allActivities = await db.Activities
                .ToListAsync();
            var userKillList = await db.ActivityLogs
                .Where(x => x.RunescapeAccount.GoodplaceUser.DiscordUserId == discordId)
                .Where(x => x.Amount > 0)
                .ToListAsync();
            var userBlocklist = await db.ActivityTaskBlockLists
                .Where(x => x.GoodplaceUser.DiscordUserId == discordId)
                .Select(x => x.Activity)
                .ToListAsync();
            var globalBlockHashSet = await db.GlobalActivityBlockLists
                .Select(x => x.ActivityId)
                .ToHashSetAsync();
            var globalActivityBlocks = allActivities
                .Where(x => globalBlockHashSet
                .Contains(x.Id))
                .ToList();
            var userWallet = await db.Wallets
                .Where(x => x.GoodplaceUser.DiscordUserId == discordId)
                .FirstAsync();
            var currentTask = await db.GoodplaceActivityTasks
                .Where(x => x.GoodplaceUser.DiscordUserId == discordId)
                .FirstOrDefaultAsync();

            Activity newTask;
            bool awardPoints = false;

            
            if (currentTask == null)
            {
                newTask = GetRandomTask<Activity>(allActivities, globalActivityBlocks, userBlocklist, allActivities.FirstOrDefault());
                GoodplaceActivityTask newActivityTask = new GoodplaceActivityTask
                {
                    GoodplaceUserId = userWallet.GoodplaceUserId,
                    CurrentCumulativeAmount = userKillList
                    .Where(x => x.Activity == newTask)
                    .Select(x => x.Amount)
                    .Aggregate(0, (a, b) => a + b),
                    // TODO add a method to generate a random goal based on kph
                    GoalAmount = 1,
                    Activity = newTask
                };
                db.GoodplaceActivityTasks.Add(newActivityTask);

            }
            else
            {
                if (userKillList
                    .Where(x => x.ActivityId == currentTask.ActivityId)
                    .Select(x => x.Amount)
                    .Aggregate(0, (a, b) => a + b) < currentTask.GoalAmount)
                {
                    // Exit if kills are lower than the goal amount (task not completed)
                    return GoodplaceTaskResult.FailureNotImplemented;
                }
                newTask = GetRandomTask<Activity>(allActivities, globalActivityBlocks, userBlocklist, allActivities.Where(x => x.Id == currentTask.ActivityId).First());
                awardPoints = true;
                // Change the task to the new one
                currentTask.CurrentCumulativeAmount = userKillList
                    .Where(x => x.ActivityId == newTask.Id)
                    .Select(x => x.Amount)
                    .Aggregate(0, (a, b) => a + b);
                // TODO add a method to generate a goal based on kph
                currentTask.GoalAmount = 1;
                currentTask.ActivityId = newTask.Id;
            }

            // Award points if a task was completed
            if (awardPoints)
            {
                userWallet.GoodplacePoints += 5;
                userWallet.GoodplaceCurrency += 5;
            }
            await db.SaveChangesAsync();

            return GoodplaceTaskResult.Success;
        }

        /// <summary>
        /// Removes a skilltask based on the tasktype
        /// </summary>
        /// <param name="discordId">Users Discord ID</param>
        /// <param name="taskType">0 for activities, 1 for Skill tasks</param>
        /// <returns></returns>
        public async Task SkipGoodplaceTask(long discordId, int taskType)
        {
            switch (taskType){
                case 0: 
                    await db.GoodplaceActivityTasks
                        .Where(x => x.GoodplaceUser.DiscordUserId == discordId)
                        .ExecuteDeleteAsync();
                    break;
                case 1:
                    await db.GoodplaceSkillTasks
                        .Where(x => x.GoodplaceUser.DiscordUserId == discordId)
                        .ExecuteDeleteAsync();
                    break;
            }
            
            // Save the deletion
            await db.SaveChangesAsync();
            

        }

        /// <summary>
        /// Adds a task to the blocklist of the user that calls it.
        /// </summary>
        /// <param name="discordId">Users Discord ID</param>
        /// <param name="taskType">0 for activities, 1 for Skill tasks</param>
        /// <returns></returns>
        public async Task BlockGoodplaceTask(long discordId, int taskType)
        {
            
            switch (taskType)
            {
                case 0:
                    var newActivityBlock = new ActivityTaskBlocklist
                    {
                        ActivityId = await db.GoodplaceActivityTasks
                                        .Where(x => x.GoodplaceUser.DiscordUserId == discordId)
                                        .Select(x => x.ActivityId)
                                        .FirstAsync(),
                        GoodplaceUserId = await db.GoodplaceUsers.Where(x => x.DiscordUserId == discordId).Select(x => x.Id).FirstAsync(),

                    };
                    db.ActivityTaskBlockLists.Add(newActivityBlock);
                    break;
                case 1:
                    var userSkillBlocklist = await db.SkillTaskBlockLists.Where(x => x.GoodplaceUser.DiscordUserId == discordId).ToListAsync();
                    var newSkillBlock = new SkillTaskBlocklist
                    {
                        GoodplaceUserId = await db.GoodplaceUsers
                            .Where(x => x.DiscordUserId == discordId)
                            .Select(x => x.Id)
                            .FirstAsync(),
                        Skill = await db.GoodplaceSkillTasks
                        .Where(x => x.GoodplaceUser.DiscordUserId == discordId)
                        .Select(x => x.Skill).FirstAsync()

                    };
                    db.SkillTaskBlockLists.Add(newSkillBlock);
                    break;
            }
            await db.SaveChangesAsync();
            // Clear the blocked Task
            await SkipGoodplaceTask(discordId, taskType);

        }

    }
}
