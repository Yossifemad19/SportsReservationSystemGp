using backend.Core.Entities;

namespace backend.Core.Specification
{
    public class FacilitySpecification : BaseSpecification<Facility>
    {
        public FacilitySpecification() : base()
        {
            AddInclude(f => f.Address);
        }

        public FacilitySpecification(int ownerId) : base(f => f.OwnerId == ownerId)
        {
            AddInclude(f => f.Address);
        }
    }

    public class FacilityWithCourtsBySportSpecification : BaseSpecification<Facility>
    {
        public FacilityWithCourtsBySportSpecification(int sportId) : base(f => f.Courts.Any(c => c.SportId == sportId))
        {
            AddInclude(f => f.Address);
            AddInclude(f => f.Courts);
        }

        public FacilityWithCourtsBySportSpecification(int ownerId, int sportId) : base(f => f.OwnerId == ownerId && f.Courts.Any(c => c.SportId == sportId))
        {
            AddInclude(f => f.Address);
            AddInclude(f => f.Courts);
        }
    }

    public class FacilityWithOwnerAndSportFilterSpecification : BaseSpecification<Facility>
    {
        public FacilityWithOwnerAndSportFilterSpecification(
            bool isOwner,
            int? ownerId = null,
            int? sportId = null,
            string? city = null)
        {
            AddInclude(f => f.Address);

            if (sportId.HasValue)
            {
                AddInclude(f => f.Courts);
            }

            Criteria = f =>
                (!isOwner || (ownerId.HasValue && f.OwnerId == ownerId.Value)) &&
                (!sportId.HasValue || f.Courts.Any(c => c.SportId == sportId.Value)) &&
                (string.IsNullOrEmpty(city) || f.Address.City.ToLower() == city.ToLower());
        }
    }

}