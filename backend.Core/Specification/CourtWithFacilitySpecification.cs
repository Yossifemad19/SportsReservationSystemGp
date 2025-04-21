using backend.Core.Entities;

namespace backend.Core.Specification;

public class CourtWithFacilitySpecification:BaseSpecification<Court>
{
    public CourtWithFacilitySpecification():base()
    {
        AddInclude(c=>c.Facility);
    }

    public CourtWithFacilitySpecification(int courtId):base(c=>c.Id == courtId)
    {
        AddInclude(c=>c.Facility);
    }
}