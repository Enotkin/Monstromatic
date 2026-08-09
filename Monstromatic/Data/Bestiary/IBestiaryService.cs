using System.Collections.ObjectModel;
using Monstromatic.Models;

namespace Monstromatic.Data.Bestiary;

public interface IBestiaryService
{
    ReadOnlyObservableCollection<BestiaryEntry> Entries { get; }

    bool TryAdd(Encounter encounter);

    bool Remove(BestiaryEntry entry);
}
