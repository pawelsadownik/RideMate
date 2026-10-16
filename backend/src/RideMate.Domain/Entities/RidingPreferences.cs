using RideMate.Domain.Common;

namespace RideMate.Domain.Entities;

public sealed class RidingPreferences : BaseEntity
{
    public Guid ProfileId { get; private set; }

    // Motocykle
    public string? MotorcycleType { get; private set; }      // np. cruiser, sport, touring, enduro
    public string? MotorcycleBrand { get; private set; }
    public string? MotorcycleModel { get; private set; }
    public bool RidesOwnMotorcycle { get; private set; }
    public bool CanTakePassenger { get; private set; }
    public bool WantsToBePassenger { get; private set; }

    // Rowery
    public bool RoadBike { get; private set; }
    public bool Gravel { get; private set; }
    public bool Mtb { get; private set; }
    public bool Trekking { get; private set; }
    public bool CityBike { get; private set; }
    public bool Recreational { get; private set; }

    public Profile? Profile { get; private set; }

    private RidingPreferences() { }

    public static RidingPreferences Create(Guid profileId) => new() { ProfileId = profileId };

    public void UpdateMotorcycle(
        string? type, string? brand, string? model,
        bool ridesOwn, bool canTakePassenger, bool wantsToBePassenger)
    {
        MotorcycleType = type;
        MotorcycleBrand = brand;
        MotorcycleModel = model;
        RidesOwnMotorcycle = ridesOwn;
        CanTakePassenger = canTakePassenger;
        WantsToBePassenger = wantsToBePassenger;
        Touch();
    }

    public void UpdateBicycles(
        bool roadBike, bool gravel, bool mtb,
        bool trekking, bool cityBike, bool recreational)
    {
        RoadBike = roadBike;
        Gravel = gravel;
        Mtb = mtb;
        Trekking = trekking;
        CityBike = cityBike;
        Recreational = recreational;
        Touch();
    }
}
