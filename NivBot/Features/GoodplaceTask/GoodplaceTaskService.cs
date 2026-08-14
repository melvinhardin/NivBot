using NivBot.DataLayer;
using NivBot.ExternalServicesLayer.OsrsAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.Features.GoodplaceTask
{
    public class GoodplaceTaskService(GoodplaceContext db, IOsrsHighscoreService osrsHighscore)
    {
        public void GetGoodplaceSkillTask(long discordId)
        {
            // See if the discord user exists in the db

            // Collect skill data on the all of the users runescape accounts

            // Check if the user has completed the existing Task and award points

            // Give a task
                
            


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
