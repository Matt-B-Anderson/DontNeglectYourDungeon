using DontNeglectYourDungeon.Data.Models;
using DontNeglectYourDungeon.Data.Services.Interfaces;

namespace DontNeglectYourDungeon.Data.QA;

public record QaResult(string Name, bool Passed, string Details = "");

public class QaSmokeTests
{
    private readonly ICampaignService _campaigns;

    public QaSmokeTests(ICampaignService campaigns)
    {
        _campaigns = campaigns;
    }

    public async Task<List<QaResult>> RunAsync(string userA, string userB)
    {
        var results = new List<QaResult>();

        // CREATE
        Campaign created;
        try
        {
            created = await _campaigns.CreateAsync(new Campaign
            {
                Name = "QA Campaign",
                System = "D&D 5e",
                Description = "Created by QA smoke test"
            }, userA);

            results.Add(new QaResult("Create campaign", created.Id > 0, $"Id={created.Id}"));
        }
        catch (Exception ex)
        {
            results.Add(new QaResult("Create campaign", false, ex.Message));
            return results;
        }

        // READ (ownership filter)
        try
        {
            var listA = await _campaigns.GetForUserAsync(userA);
            var listB = await _campaigns.GetForUserAsync(userB);

            results.Add(new QaResult("Read campaigns for owner", listA.Any(c => c.Id == created.Id)));
            results.Add(new QaResult("Read campaigns for other user (should NOT see)", !listB.Any(c => c.Id == created.Id)));
        }
        catch (Exception ex)
        {
            results.Add(new QaResult("Read campaigns", false, ex.Message));
        }

        // UPDATE allowed for owner
        try
        {
            var ok = await _campaigns.UpdateAsync(new Campaign
            {
                Id = created.Id,
                Name = "QA Campaign (Renamed)",
                System = created.System,
                Description = created.Description
            }, userA);

            results.Add(new QaResult("Update campaign as owner", ok));
        }
        catch (Exception ex)
        {
            results.Add(new QaResult("Update campaign as owner", false, ex.Message));
        }

        // UPDATE blocked for non-owner
        try
        {
            var ok = await _campaigns.UpdateAsync(new Campaign
            {
                Id = created.Id,
                Name = "HACKED",
                System = created.System,
                Description = created.Description
            }, userB);

            results.Add(new QaResult("Update campaign as non-owner (should fail)", ok == false));
        }
        catch
        {
            // If your service throws instead of returning false, that’s also “blocked” behavior.
            results.Add(new QaResult("Update campaign as non-owner (should fail)", true, "Blocked via exception"));
        }

        // DELETE blocked for non-owner
        try
        {
            var ok = await _campaigns.DeleteAsync(created.Id, userB);
            results.Add(new QaResult("Delete campaign as non-owner (should fail)", ok == false));
        }
        catch
        {
            results.Add(new QaResult("Delete campaign as non-owner (should fail)", true, "Blocked via exception"));
        }

        // DELETE allowed for owner
        try
        {
            var ok = await _campaigns.DeleteAsync(created.Id, userA);
            results.Add(new QaResult("Delete campaign as owner", ok));
        }
        catch (Exception ex)
        {
            results.Add(new QaResult("Delete campaign as owner", false, ex.Message));
        }

        return results;
    }
}
