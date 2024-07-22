using Monstromatic.Models;

namespace Monstromatic.Services;

public interface IEncounterFactory
{
    public Encounter CreateEncounter();
}