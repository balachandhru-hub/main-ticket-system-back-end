using Entities.Common;

namespace Dto
{
    public class PRUpdateDto
    {
        public string PRType { get; set; } = Common.PR_MATERIAL_TYPE; // Default to MATERIAL, can be set to SERVICE for service PRs
        public string PRNumber { get; set; }
        public List<LineItem> LineItems { get; set; }

    }

    public class LineItem
    {
        public string LineNumber { get; set;}
        public double Price { get; set;}
    }
}