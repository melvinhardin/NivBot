using Microsoft.EntityFrameworkCore;
using NivBot.DataLayer;
using NivBot.DataLayer.Models;
using NivBot.ExternalServicesLayer.OsrsAPI;
using NivBot.ExternalServicesLayer.OsrsAPI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NivBot.Features.SyncActivities
{
    public class SyncActivitiesService(GoodplaceContext db, IOsrsHighscoreService osrsApi)
    {
        public async Task<SyncActivitiesResult> SyncActivities()
        {
            // Get all the activities from the runescape api
            PlayerStats? player = await osrsApi.GetPlayerStatsAsync("niv lem");
            if (player == null) { return SyncActivitiesResult.FailureOsrsApiConnection; }

            // Get all the activities currently in the database
            List<DataLayer.Models.Activity> activities;
            try { activities = await db.Activities.ToListAsync(); }
            catch (Exception ex) { return SyncActivitiesResult.FailureDbConnection; }

            // Turning both lists to dictionary sorted by the osrsnames.
            var apiToName = player.Activities.ToDictionary(x => x.Name);
            var dbToName = activities.ToDictionary(x => x.OsrsName);

            // Compare them on names
            var addedActivities = apiToName.Keys.Except(dbToName.Keys).ToList();
            var removedOrRenamedActivities = dbToName.Keys.Except(apiToName.Keys).ToList();

            // Alert and fail if names in the database but no longer on the API (Either removed or renamed)
            if (removedOrRenamedActivities.Count > 0) { return SyncActivitiesResult.FailureNameChange; }

            // Note if there were no changes between the db and api.
            if (addedActivities.Count == 0) { return SyncActivitiesResult.SuccessNoChanges; }

            // Update all the existing osrsId's of items
            foreach (var i in activities.ToList())
            {
                i.OsrsId = apiToName[i.OsrsName].Id;
            }

            // Add new Activities 
            List<NivBot.DataLayer.Models.Activity> newActivities = addedActivities.Select(x => new NivBot.DataLayer.Models.Activity
            {
                OsrsId = apiToName[x].Id,
                OsrsName = apiToName[x].Name

            }).ToList();
            db.Activities.AddRange(newActivities);
            try { await db.SaveChangesAsync(); }
            catch (Exception ex){ return SyncActivitiesResult.FailureDbSave; }
            
            // Return Success state
            return SyncActivitiesResult.Success;
        }
    }
}
