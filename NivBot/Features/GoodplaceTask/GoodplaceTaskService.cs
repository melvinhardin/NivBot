using Microsoft.EntityFrameworkCore;
using NivBot.DataLayer;
using NivBot.DataLayer.Enums;
using NivBot.DataLayer.Models;
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
                newTask = GetRandomTask<Skills>(availableTasks, globalBlockList, userBlockList, Skills.Attack);
                awardPoints = true;

                // Change the task to the new one
                currentTask.SummedCurrentXp = xpList
                    .Where(x => x.Skill == newTask)
                    .Select(x => x.Xp)
                    .Aggregate(0L, (a, b) => a + b);
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

            // Get the current total

            Console.WriteLine();

            return GoodplaceTaskResult.FailureNotImplemented;
        }

        // A Helper method to return a random task, generic so that it works for skill or activity tasks.
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

        public async Task GetGoodplaceBossTask(long discordId)
        {
            var allActivites = await db.Activities.ToListAsync();

            // Get the global and userblocklists.
            var userWallet = await db.Wallets
                .Where(x => x.GoodplaceUser.DiscordUserId == discordId)
                .ToListAsync();
            var userBlocklist = await db.ActivityTaskBlockLists
                .Where(x => x.GoodplaceUser.DiscordUserId == discordId)
                .Select(x => x.Activity).ToListAsync();
            var globalBlocklist = await db.Activities.Where(x => userBlocklist.Contains(x.Id));



            

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
