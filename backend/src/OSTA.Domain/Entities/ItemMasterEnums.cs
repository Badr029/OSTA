namespace OSTA.Domain.Entities;

public enum ItemType
{
    FinishedGood = 1,
    Assembly = 2,
    Part = 3,
    Component = 4,
    HardwareKit = 5
}

public enum ProcurementType
{
    Make = 1,
    Buy = 2,
    Phantom = 3,
    Subcontract = 4
}
