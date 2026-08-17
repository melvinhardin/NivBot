using Microsoft.EntityFrameworkCore;
using NivBot.DataLayer;
using NivBot.DataLayer.Enums;
using NivBot.ExternalServicesLayer.OsrsAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NivBot.Features.GoodplaceTask
{
    public class GoodplaceTaskService(GoodplaceContext db, IOsrsHighscoreService osrsHighscore)
    {
        public async Task<GoodplaceTaskResult> GetGoodplaceSkillTask(long discordId)
        {
            // Get All available skill tasks (all skills)
            var availableTasks = Enum
                .GetValues(typeof(Skills))
                .Cast<Skills>()
                .ToList();

            // Global skill blocklist
            List<Skills> globalBlockList = new List<Skills> { Skills.Attack, Skills.Defence, Skills.Strength, Skills.Magic, Skills.Hitpoints, Skills.Ranged, Skills.Prayer };


            // Get the user blocklist
            var userBlockList = await db.SkillTaskBlockLists
                .Where(x => x.GoodplaceUser.DiscordUserId == discordId)
                .Select(x => x.Skill)
                .ToListAsync();
            
            // See if the discord user exists in the db
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

            // Check if the user has completed the existing Task and award points

            if (currentTask != null)
            {
                // Sum the xp (as a long because it's over all their account)
                // Compare the aggregated data against the goal xp
                // Exit if xp is lower than the goal amount (task not completed)
                if (
                    xpList
                    .Where(x => x.Skill == currentTask.Skill)
                    .Select(x => x.Xp)
                    .Aggregate(0L, (a,b) => a+b) < currentTask.GoalXp
                    )
                {
                    return GoodplaceTaskResult.FailureNotImplemented;
                }

                Console.WriteLine(currentTask.GoalXp);
            }
            
            // A very silly workaround for the first task of the user
            // We set the current task to one in the global blocklist, the method checks for this
            // Not very efficient, but this is what I came up with, with my current C# knowledge
            var newTask = GetRandomTask<Skills>(availableTasks,globalBlockList,userBlockList, Skills.Attack);
            Console.WriteLine(newTask);

            Console.WriteLine(xpList
                    .Where(x => x.Skill == newTask)
                    .Select(x => x.Xp)
                    .Aggregate(0L, (a, b) => a + b));
            
            



            return GoodplaceTaskResult.FailureNotImplemented;
        }

        // A Helper method to return a random task, generic so that it works with skill or activity tasks.
        static private T GetRandomTask<T>(List<T> availableTasks, List<T> globalBlocklist, List<T> userBlocklist, T currentTask)
        {
            // For first task the current task will always be in the global blocklist, remove the currenttask
            if (!globalBlocklist.Contains(currentTask)) { availableTasks.Remove(currentTask); }

            // Removing all tasks in the blocklists
            availableTasks = availableTasks.Except(globalBlocklist).Except(userBlocklist).ToList();

            // New random seed and return a random task
            Random rnd = new Random();
            return availableTasks[rnd.Next(availableTasks.Count)];
        }

        public void GetGoodplaceBossTask(long discordId)
        {
            // See if the discord user exists in the db

            // Collect activity data on the all of the users runescape accounts

            // Check if the user has completed the existing Task and award points

            // Give a task
        }

        public void SkipGoodplaceTask(int taskId)
        {
            // Check which task

            // Clear the current task
            
        }
        public void BlockGoodplaceTask()
        {
            // Check which task
            
            // Add the current task to the userTaskBlockList
            
            // Skip the current task
        }

    }
}
