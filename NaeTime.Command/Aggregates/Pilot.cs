using EventDbLite.Aggregates;
using NaeTime.Events.Domain;
using System.Security.Cryptography;
using System.Text;

namespace NaeTime.Command.Aggregates;
public class Pilot : AggregateRoot<Guid>
{
    public Pilot()
    {

    }
    public Pilot(Guid id)
    {
        Raise(new PilotRegistered(id));
    }

    private void When(PilotRegistered registered)
    {
        Id = registered.PilotId;
    }

    public void ChangeCallsign(string? callsign)
    {
        Raise(new PilotCallsignAssigned(Id, callsign));
    }
    public void ChangeBindingPhrase(string bindingPhrase)
    {
        byte[] hashPhrase = HashBindingPhrase(bindingPhrase);

        Raise(new PilotBindingPhraseChanged(Id, hashPhrase));
    }
    private static byte[] HashBindingPhrase(string bindPhrase)
    {
        // Create the input string
        string input = $"-DMY_BINDING_PHRASE=\"{bindPhrase}\"";

        // Compute the MD5 hash
        using MD5 md5 = MD5.Create();
        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));

        // Take the first 6 bytes of the hash
        byte[] bindingPhraseHash = hash.Take(6).ToArray();

        // Adjust the first byte if it is odd
        if ((bindingPhraseHash[0] % 2) == 1)
        {
            bindingPhraseHash[0] -= 0x01;
        }

        return bindingPhraseHash;
    }
    public void RemoveBindingPhrase()
    {
        Raise(new PilotBindingPhraseRemoved(Id));
    }
    public void RenamePilot(string? firstName, string? lastName)
    {
        Raise(new PilotRenamed(Id, firstName, lastName));
    }
}
