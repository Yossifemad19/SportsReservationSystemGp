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
        public FacilityWithOwnerAndSportFilterSpecification(bool isOwner, int? ownerId = null, int? sportId = null)
        {
            AddInclude(f => f.Address);
            
            // Always include Courts when filtering by sport to ensure proper query execution
            if (sportId.HasValue)
            {
                AddInclude(f => f.Courts);
            }

            if (isOwner && ownerId.HasValue)
            {
                if (sportId.HasValue)
                {
                    Criteria = f => f.OwnerId == ownerId.Value && f.Courts.Any(c => c.SportId == sportId.Value);
                }
                else
                {
                    Criteria = f => f.OwnerId == ownerId.Value;
                }
            }
            else
            {
                if (sportId.HasValue)
                {
                    Criteria = f => f.Courts.Any(c => c.SportId == sportId.Value);
                }
                else
                {
                    Criteria = f => true; // All facilities
                }
            }
        }
    }
} 