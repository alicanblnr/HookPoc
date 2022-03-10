namespace Infra.Dto
{

        public class ApplicantDto
    {
        public ApplicantDto()
        { }
        public string Id { get; set; }
        public string CreatedAt { get; set; }
        public string ClientId { get; set; }
        public string InspectionId { get; set; }
        public string ExternalUserId { get; set; }
        public FixedInfo FixedInfo { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public RequiredIdDocs RequiredIdDocs { get; set; }
        public Review Review { get; set; }
        public string Type { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class FixedInfo
    {
        public FixedInfo()
        { }
        public string PlaceOfBirth { get; set; }
        public string Country { get; set; }
    }

    public class DocSet
    {
        public DocSet()
        {
            
        }
        public string IdDocSetType { get; set; }
        public List<string>Ttypes { get; set; }
        public string VideoRequired { get; set; }
    }

    public class RequiredIdDocs
    {
        public RequiredIdDocs()
        {
            
        }
        public List<string> ExcludedCountries { get; set; }
        public List<DocSet> DocSets { get; set; }
    }

    public class Review
    {
        public Review()
        { }
        public bool Reprocessing { get; set; }
        public string CreateDate { get; set; }
        public string ReviewStatus { get; set; }
    }

}