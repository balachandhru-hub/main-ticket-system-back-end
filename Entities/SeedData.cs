using Entities.DbContext;
using Microsoft.Extensions.DependencyInjection;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;
using Entities.Models;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Runtime.InteropServices;


namespace Entities
{
    /// <remarks>
    /// This method is excluded from code coverage analysis as it is used internally for seed the data to all the table for
    /// the Integration testing and does not contribute to the public API surface.
    /// </remarks>
    public static class SeedData
    {

        public static void CreateRole(Stream stream, RepositoryContext context)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreReferences = true
                };
                CsvReader csvReader = new CsvReader(reader, conf);
                System.Collections.Generic.IEnumerable<Role> entries = csvReader.GetRecords<Role>();
                List<Role> csvCount = entries.ToList();
                foreach (Role entry in csvCount)
                {
                    var existingEntry = context.Role.Find(entry.Id);

                    if (existingEntry != null)
                    {
                        // Backup original values
                        var originalValues = context.Entry(existingEntry).OriginalValues;

                        // Update existing entry
                        context.Entry(existingEntry).CurrentValues.SetValues(entry);

                        // Restore original values for specific fields
                        existingEntry.DateCreated = originalValues.GetValue<DateTime>("DateCreated");
                        existingEntry.CreatedBy = originalValues.GetValue<Guid>("CreatedBy");
                        existingEntry.IsActive = originalValues.GetValue<bool>("IsActive");

                    }
                    else
                    {
                        // Add new entry
                        _ = context.Role.Add(entry);
                    }
                }
                SaveEntities(context);
                csvReader.Dispose();
            }
        }

        public static void CreatePerson(Stream stream, RepositoryContext context)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreReferences = true
                };
                CsvReader csvReader = new CsvReader(reader, conf);
                System.Collections.Generic.IEnumerable<Person> entries = csvReader.GetRecords<Person>();
                List<Person> csvCount = entries.ToList();
                foreach (Person entry in csvCount)
                {
                    var existingEntry = context.Person.Find(entry.Id);

                    if (existingEntry != null)
                    {
                        // Backup original values
                        var originalValues = context.Entry(existingEntry).OriginalValues;

                        // Update existing entry
                        context.Entry(existingEntry).CurrentValues.SetValues(entry);

                        // Restore original values for specific fields
                        existingEntry.DateCreated = originalValues.GetValue<DateTime>("DateCreated");
                        existingEntry.CreatedBy = originalValues.GetValue<Guid>("CreatedBy");
                        existingEntry.IsActive = originalValues.GetValue<bool>("IsActive");

                    }
                    else
                    {
                        // Add new entry
                        _ = context.Person.Add(entry);
                    }
                }
                SaveEntities(context);
                csvReader.Dispose();
            }
        }

        public static void CreateUser(Stream stream, RepositoryContext context)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreReferences = true
                };
                CsvReader csvReader = new CsvReader(reader, conf);
                System.Collections.Generic.IEnumerable<User> entries = csvReader.GetRecords<User>();
                List<User> csvCount = entries.ToList();
                foreach (User entry in csvCount)
                {
                    var existingEntry = context.User.Find(entry.Id);

                    if (existingEntry != null)
                    {
                        // Backup original values
                        var originalValues = context.Entry(existingEntry).OriginalValues;

                        // Update existing entry
                        context.Entry(existingEntry).CurrentValues.SetValues(entry);

                        // Restore original values for specific fields
                        existingEntry.DateCreated = originalValues.GetValue<DateTime>("DateCreated");
                        existingEntry.CreatedBy = originalValues.GetValue<Guid>("CreatedBy");
                        existingEntry.IsActive = originalValues.GetValue<bool>("IsActive");

                    }
                    else
                    {
                        // Add new entry
                        _ = context.User.Add(entry);
                    }
                }
                SaveEntities(context);
                csvReader.Dispose();
            }
        }

        public static void CreateUserRoleMapping(Stream stream, RepositoryContext context)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreReferences = true
                };
                CsvReader csvReader = new CsvReader(reader, conf);
                System.Collections.Generic.IEnumerable<UserRoleMapping> entries = csvReader.GetRecords<UserRoleMapping>();
                List<UserRoleMapping> csvCount = entries.ToList();
                foreach (UserRoleMapping entry in csvCount)
                {
                    var existingEntry = context.UserRoleMapping.Find(entry.Id);

                    if (existingEntry != null)
                    {
                        // Backup original values
                        var originalValues = context.Entry(existingEntry).OriginalValues;

                        // Update existing entry
                        context.Entry(existingEntry).CurrentValues.SetValues(entry);

                        // Restore original values for specific fields
                        existingEntry.DateCreated = originalValues.GetValue<DateTime>("DateCreated");
                        existingEntry.CreatedBy = originalValues.GetValue<Guid>("CreatedBy");
                        existingEntry.IsActive = originalValues.GetValue<bool>("IsActive");

                    }
                    else
                    {
                        // Add new entry
                        _ = context.UserRoleMapping.Add(entry);
                    }
                }
                SaveEntities(context);
                csvReader.Dispose();
            }
        }

        public static void CreateRoleFeatureMapping(Stream stream, RepositoryContext context)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreReferences = true
                };
                CsvReader csvReader = new CsvReader(reader, conf);
                System.Collections.Generic.IEnumerable<RoleFeatureMapping> entries = csvReader.GetRecords<RoleFeatureMapping>();
                List<RoleFeatureMapping> csvCount = entries.ToList();
                foreach (RoleFeatureMapping entry in csvCount)
                {
                    var existingEntry = context.RoleFeatureMapping.Find(entry.Id);

                    if (existingEntry != null)
                    {
                        // Backup original values
                        var originalValues = context.Entry(existingEntry).OriginalValues;

                        // Update existing entry
                        context.Entry(existingEntry).CurrentValues.SetValues(entry);

                        // Restore original values for specific fields
                        existingEntry.DateCreated = originalValues.GetValue<DateTime>("DateCreated");
                        existingEntry.CreatedBy = originalValues.GetValue<Guid>("CreatedBy");
                        existingEntry.IsActive = originalValues.GetValue<bool>("IsActive");

                    }
                    else
                    {
                        // Add new entry
                        _ = context.RoleFeatureMapping.Add(entry);
                    }
                }
                SaveEntities(context);
                csvReader.Dispose();
            }
        }

        public static void CreateMetadata(Stream stream, RepositoryContext context)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreReferences = true
                };
                CsvReader csvReader = new CsvReader(reader, conf);
                System.Collections.Generic.IEnumerable<Metadata> entries = csvReader.GetRecords<Metadata>();
                List<Metadata> csvCount = entries.ToList();
                foreach (Metadata entry in csvCount)
                {
                    var existingEntry = context.Metadata.Find(entry.Id);

                    if (existingEntry != null)
                    {
                        // Backup original values
                        var originalValues = context.Entry(existingEntry).OriginalValues;

                        // Update existing entry
                        context.Entry(existingEntry).CurrentValues.SetValues(entry);

                        // Restore original values for specific fields
                        existingEntry.DateCreated = originalValues.GetValue<DateTime>("DateCreated");
                        existingEntry.CreatedBy = originalValues.GetValue<Guid>("CreatedBy");
                        existingEntry.IsActive = originalValues.GetValue<bool>("IsActive");

                    }
                    else
                    {
                        // Add new entry
                        _ = context.Metadata.Add(entry);
                    }
                }
                SaveEntities(context);
                csvReader.Dispose();
            }
        }

        public static void CreateFeature(Stream stream, RepositoryContext context)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreReferences = true
                };
                CsvReader csvReader = new CsvReader(reader, conf);
                System.Collections.Generic.IEnumerable<Feature> entries = csvReader.GetRecords<Feature>();
                List<Feature> csvCount = entries.ToList();
                foreach (Feature entry in csvCount)
                {
                    var existingEntry = context.Feature.Find(entry.Id);

                    if (existingEntry != null)
                    {
                        // Backup original values
                        var originalValues = context.Entry(existingEntry).OriginalValues;

                        // Update existing entry
                        context.Entry(existingEntry).CurrentValues.SetValues(entry);

                        // Restore original values for specific fields
                        existingEntry.DateCreated = originalValues.GetValue<DateTime>("DateCreated");
                        existingEntry.CreatedBy = originalValues.GetValue<Guid>("CreatedBy");
                        existingEntry.IsActive = originalValues.GetValue<bool>("IsActive");

                    }
                    else
                    {
                        // Add new entry
                        _ = context.Feature.Add(entry);
                    }
                }
                SaveEntities(context);
                csvReader.Dispose();
            }
        }

        public static void CreateApiKey(Stream stream, RepositoryContext context)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreReferences = true
                };
                CsvReader csvReader = new CsvReader(reader, conf);
                System.Collections.Generic.IEnumerable<ApiKey> entries = csvReader.GetRecords<ApiKey>();
                List<ApiKey> csvCount = entries.ToList();
                foreach (ApiKey entry in csvCount)
                {
                    var existingEntry = context.ApiKey.Find(entry.Id);

                    if (existingEntry != null)
                    {
                        // Backup original values
                        var originalValues = context.Entry(existingEntry).OriginalValues;

                        // Update existing entry
                        context.Entry(existingEntry).CurrentValues.SetValues(entry);

                        // Restore original values for specific fields
                        existingEntry.DateCreated = originalValues.GetValue<DateTime>("DateCreated");
                        existingEntry.CreatedBy = originalValues.GetValue<Guid>("CreatedBy");
                        existingEntry.IsActive = originalValues.GetValue<bool>("IsActive");

                    }
                    else
                    {
                        // Add new entry
                        _ = context.ApiKey.Add(entry);
                    }
                }
                SaveEntities(context);
                csvReader.Dispose();
            }
        }

        public static void CreateConfig(Stream stream, RepositoryContext context)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreReferences = true
                };
                CsvReader csvReader = new CsvReader(reader, conf);
                System.Collections.Generic.IEnumerable<Config> entries = csvReader.GetRecords<Config>();
                List<Config> csvCount = entries.ToList();
                foreach (Config entry in csvCount)
                {
                    var existingEntry = context.Config.Find(entry.Id);

                    if (existingEntry != null)
                    {
                        // Backup original values
                        var originalValues = context.Entry(existingEntry).OriginalValues;

                        // Update existing entry
                        context.Entry(existingEntry).CurrentValues.SetValues(entry);

                        // Restore original values for specific fields
                        existingEntry.DateCreated = originalValues.GetValue<DateTime>("DateCreated");
                        existingEntry.CreatedBy = originalValues.GetValue<Guid>("CreatedBy");
                        existingEntry.IsActive = originalValues.GetValue<bool>("IsActive");

                    }
                    else
                    {
                        // Add new entry
                        _ = context.Config.Add(entry);
                    }
                }
                SaveEntities(context);
                csvReader.Dispose();
            }
        }

        public static void CreateDomain(Stream stream, RepositoryContext context)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreReferences = true
                };
                CsvReader csvReader = new CsvReader(reader, conf);
                System.Collections.Generic.IEnumerable<Domain> entries = csvReader.GetRecords<Domain>();
                List<Domain> csvCount = entries.ToList();
                foreach (Domain entry in csvCount)
                {
                    var existingEntry = context.Domain.Find(entry.Id);

                    if (existingEntry != null)
                    {
                        // Backup original values
                        var originalValues = context.Entry(existingEntry).OriginalValues;

                        // Update existing entry
                        context.Entry(existingEntry).CurrentValues.SetValues(entry);

                        // Restore original values for specific fields
                        existingEntry.DateCreated = originalValues.GetValue<DateTime>("DateCreated");
                        existingEntry.CreatedBy = originalValues.GetValue<Guid>("CreatedBy");
                        existingEntry.IsActive = originalValues.GetValue<bool>("IsActive");

                    }
                    else
                    {
                        // Add new entry
                        _ = context.Domain.Add(entry);
                    }
                }
                SaveEntities(context);
                csvReader.Dispose();
            }
        }

        public static void CreateApiConfig(Stream stream, RepositoryContext context)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreReferences = true
                };
                CsvReader csvReader = new CsvReader(reader, conf);
                System.Collections.Generic.IEnumerable<ApiConfig> entries = csvReader.GetRecords<ApiConfig>();
                List<ApiConfig> csvCount = entries.ToList();
                foreach (ApiConfig entry in csvCount)
                {
                    var existingEntry = context.ApiConfig.Find(entry.Id);

                    if (existingEntry != null)
                    {
                        // Backup original values
                        var originalValues = context.Entry(existingEntry).OriginalValues;

                        // Update existing entry
                        context.Entry(existingEntry).CurrentValues.SetValues(entry);

                        // Restore original values for specific fields
                        existingEntry.DateCreated = originalValues.GetValue<DateTime>("DateCreated");
                        existingEntry.CreatedBy = originalValues.GetValue<Guid>("CreatedBy");
                        existingEntry.IsActive = originalValues.GetValue<bool>("IsActive");

                    }
                    else
                    {
                        // Add new entry
                        _ = context.ApiConfig.Add(entry);
                    }
                }
                SaveEntities(context);
                csvReader.Dispose();
            }
        }
        /// <summary>
        /// Reads PlanConfigDefaults.csv and seeds PlanConfig table.
        /// </summary>
        public static void CreatePlanConfigDefaults(Stream stream, RepositoryContext context)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreReferences = true
                };
                CsvReader csvReader = new CsvReader(reader, conf);
                System.Collections.Generic.IEnumerable<PlanConfig> entries = csvReader.GetRecords<PlanConfig>();
                List<PlanConfig> csvCount = entries.ToList();
                foreach (PlanConfig entry in csvCount)
                {
                    // Try to find an existing entry by Plan, EngagementType, and Key
                    var existingEntry = context.PlanConfig.Find(entry.Id);
                    if (existingEntry != null)
                    {
                        // Backup original values
                        var originalValues = context.Entry(existingEntry).OriginalValues;
                        // Update existing entry
                        context.Entry(existingEntry).CurrentValues.SetValues(entry);
                        // Restore original values for specific fields
                        existingEntry.DateCreated = originalValues.GetValue<DateTime>("DateCreated");
                        existingEntry.CreatedBy = originalValues.GetValue<Guid>("CreatedBy");
                        existingEntry.IsActive = originalValues.GetValue<bool>("IsActive");
                    }
                    else
                    {
                        // Add new entry
                        _ = context.PlanConfig.Add(entry);
                    }
                }
                SaveEntities(context);
                csvReader.Dispose();
            }
        }

        public static void CreateDiscoutCoupn(Stream stream, RepositoryContext context)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreReferences = true
                };
                CsvReader csvReader = new CsvReader(reader, conf);
                System.Collections.Generic.IEnumerable<DiscountCoupon> entries = csvReader.GetRecords<DiscountCoupon>();
                List<DiscountCoupon> csvCount = entries.ToList();
                foreach (DiscountCoupon entry in csvCount)
                {
                    // Try to find an existing entry by Plan, EngagementType, and Key
                    var existingEntry = context.DiscountCoupon.Find(entry.Id);
                    if (existingEntry != null)
                    {
                        // Backup original values
                        var originalValues = context.Entry(existingEntry).OriginalValues;
                        // Update existing entry
                        context.Entry(existingEntry).CurrentValues.SetValues(entry);
                        // Restore original values for specific fields
                        existingEntry.DateCreated = originalValues.GetValue<DateTime>("DateCreated");
                        existingEntry.CreatedBy = originalValues.GetValue<Guid>("CreatedBy");
                        existingEntry.IsActive = originalValues.GetValue<bool>("IsActive");
                    }
                    else
                    {
                        // Add new entry
                        _ = context.DiscountCoupon.Add(entry);
                    }
                }
                SaveEntities(context);
                csvReader.Dispose();
            }
        }

        public static void CreateEmailContent(Stream stream, RepositoryContext context)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreReferences = true,
                    BadDataFound = null
                };
                CsvReader csvReader = new CsvReader(reader, conf);
                System.Collections.Generic.IEnumerable<EmailContent> entries = csvReader.GetRecords<EmailContent>();
                List<EmailContent> csvCount = entries.ToList();
                foreach (EmailContent entry in csvCount)
                {
                    // Try to find an existing entry by Plan, EngagementType, and Key
                    var existingEntry = context.EmailContent.Find(entry.Id);
                    if (existingEntry != null)
                    {
                        // Backup original values
                        var originalValues = context.Entry(existingEntry).OriginalValues;
                        // Update existing entry
                        context.Entry(existingEntry).CurrentValues.SetValues(entry);
                        // Restore original values for specific fields
                        existingEntry.DateCreated = originalValues.GetValue<DateTime>("DateCreated");
                        existingEntry.CreatedBy = originalValues.GetValue<Guid>("CreatedBy");
                        existingEntry.IsActive = originalValues.GetValue<bool>("IsActive");
                    }
                    else
                    {
                        // Add new entry
                        _ = context.EmailContent.Add(entry);
                    }
                }
                SaveEntities(context);
                csvReader.Dispose();
            }
        }
        public static void CreateLink(Stream stream, RepositoryContext context)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreReferences = true
                };
                CsvReader csvReader = new CsvReader(reader, conf);
                System.Collections.Generic.IEnumerable<Link> entries = csvReader.GetRecords<Link>();
                List<Link> csvCount = entries.ToList();
                foreach (Link entry in csvCount)
                {
                    var existingEntry = context.Set<Link>().Find(entry.Id);

                    if (existingEntry != null)
                    {
                        var originalValues = context.Entry(existingEntry).OriginalValues;
                        context.Entry(existingEntry).CurrentValues.SetValues(entry);
                        existingEntry.DateCreated = originalValues.GetValue<DateTime>("DateCreated");
                        existingEntry.CreatedBy = originalValues.GetValue<Guid>("CreatedBy");
                        existingEntry.IsActive = originalValues.GetValue<bool>("IsActive");
                    }
                    else
                    {
                        _ = context.Set<Link>().Add(entry);
                    }
                }
                SaveEntities(context);
                csvReader.Dispose();
            }
        }
        public static void CreateContractCustomType(Stream stream, RepositoryContext context)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreReferences = true
                };
                CsvReader csvReader = new CsvReader(reader, conf);
                System.Collections.Generic.IEnumerable<ContractCustomType> entries = csvReader.GetRecords<ContractCustomType>();
                List<ContractCustomType> csvCount = entries.ToList();
                foreach (ContractCustomType entry in csvCount)
                {
                    var existingEntry = context.Set<ContractCustomType>().Find(entry.Id);

                    if (existingEntry != null)
                    {
                        var originalValues = context.Entry(existingEntry).OriginalValues;
                        context.Entry(existingEntry).CurrentValues.SetValues(entry);
                        existingEntry.DateCreated = originalValues.GetValue<DateTime>("DateCreated");
                        existingEntry.CreatedBy = originalValues.GetValue<Guid>("CreatedBy");
                        existingEntry.IsActive = originalValues.GetValue<bool>("IsActive");
                    }
                    else
                    {
                        _ = context.Set<ContractCustomType>().Add(entry);
                    }
                }
                SaveEntities(context);
                csvReader.Dispose();
            }
        }

        public static void CreateUserModelName(Stream stream, RepositoryContext context)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreReferences = true,
                    BadDataFound = null
                };
                CsvReader csvReader = new CsvReader(reader, conf);
                System.Collections.Generic.IEnumerable<UserModelName> entries = csvReader.GetRecords<UserModelName>();
                List<UserModelName> csvCount = entries.ToList();
                foreach (UserModelName entry in csvCount)
                {
                    // Try to find an existing entry by Plan, EngagementType, and Key
                    var existingEntry = context.UserModelName.Find(entry.Id);
                    if (existingEntry != null)
                    {
                        // Backup original values
                        var originalValues = context.Entry(existingEntry).OriginalValues;
                        // Update existing entry
                        context.Entry(existingEntry).CurrentValues.SetValues(entry);
                        // Restore original values for specific fields
                        existingEntry.DateCreated = originalValues.GetValue<DateTime>("DateCreated");
                        existingEntry.CreatedBy = originalValues.GetValue<Guid>("CreatedBy");
                        existingEntry.IsActive = originalValues.GetValue<bool>("IsActive");
                    }
                    else
                    {
                        // Add new entry
                        _ = context.UserModelName.Add(entry);
                    }
                }
                SaveEntities(context);
                csvReader.Dispose();
            }
        }
        public static void CreateAribaCurrencyName(Stream stream, RepositoryContext context)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreReferences = true,
                    BadDataFound = null
                };
                CsvReader csvReader = new CsvReader(reader, conf);
                System.Collections.Generic.IEnumerable<AribaCurrency> entries = csvReader.GetRecords<AribaCurrency>();
                List<AribaCurrency> csvCount = entries.ToList();
                foreach (AribaCurrency entry in csvCount)
                {
                    // Try to find an existing entry by Plan, EngagementType, and Key
                    var existingEntry = context.AribaCurrency.Find(entry.Id);
                    if (existingEntry != null)
                    {
                        // Backup original values
                        var originalValues = context.Entry(existingEntry).OriginalValues;
                        // Update existing entry
                        context.Entry(existingEntry).CurrentValues.SetValues(entry);
                        // Restore original values for specific fields
                        existingEntry.DateCreated = originalValues.GetValue<DateTime>("DateCreated");
                        existingEntry.CreatedBy = originalValues.GetValue<Guid>("CreatedBy");
                        existingEntry.IsActive = originalValues.GetValue<bool>("IsActive");
                    }
                    else
                    {
                        // Add new entry
                        _ = context.AribaCurrency.Add(entry);
                    }
                }
                SaveEntities(context);
                csvReader.Dispose();
            }
        }
        public static void CreateCountryList(Stream stream, RepositoryContext context)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreReferences = true,
                    BadDataFound = null
                };
                CsvReader csvReader = new CsvReader(reader, conf);
                System.Collections.Generic.IEnumerable<CountryList> entries = csvReader.GetRecords<CountryList>();
                List<CountryList> csvCount = entries.ToList();
                foreach (CountryList entry in csvCount)
                {
                    // Try to find an existing entry by Plan, EngagementType, and Key
                    var existingEntry = context.CountryList.Find(entry.Id);
                    if (existingEntry != null)
                    {
                        // Backup original values
                        var originalValues = context.Entry(existingEntry).OriginalValues;
                        // Update existing entry
                        context.Entry(existingEntry).CurrentValues.SetValues(entry);
                        // Restore original values for specific fields
                        existingEntry.DateCreated = originalValues.GetValue<DateTime>("DateCreated");
                        existingEntry.CreatedBy = originalValues.GetValue<Guid>("CreatedBy");
                        existingEntry.IsActive = originalValues.GetValue<bool>("IsActive");
                    }
                    else
                    {
                        // Add new entry
                        _ = context.CountryList.Add(entry);
                    }
                }
                SaveEntities(context);
                csvReader.Dispose();
            }
        }

        public static void CreateUserAppMapping(Stream stream, RepositoryContext context)
        {
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                var conf = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    IgnoreReferences = true
                };
                CsvReader csvReader = new CsvReader(reader, conf);
                System.Collections.Generic.IEnumerable<UserAppMapping> entries = csvReader.GetRecords<UserAppMapping>();
                List<UserAppMapping> csvCount = entries.ToList();
                foreach (UserAppMapping entry in csvCount)
                {
                    var existingEntry = context.UserAppMapping.Find(entry.Id);

                    if (existingEntry != null)
                    {
                        // Backup original values
                        var originalValues = context.Entry(existingEntry).OriginalValues;

                        // Update existing entry
                        context.Entry(existingEntry).CurrentValues.SetValues(entry);

                        // Restore original values for specific fields
                        existingEntry.DateCreated = originalValues.GetValue<DateTime>("DateCreated");
                        existingEntry.CreatedBy = originalValues.GetValue<Guid>("CreatedBy");
                        existingEntry.IsActive = originalValues.GetValue<bool>("IsActive");

                    }
                    else
                    {
                        // Add new entry
                        _ = context.UserAppMapping.Add(entry);
                    }
                }
                SaveEntities(context);
                csvReader.Dispose();
            }
        }




        // Removed PlanConfigDefault helper class. Now using PlanConfig model directly for CSV mapping.

        public static void Initialize(IServiceProvider serviceProvider)
        {
            RepositoryContext context = serviceProvider.GetRequiredService<RepositoryContext>();
            context.Database.EnsureCreated();

            string basePath = Path.Combine(AppContext.BaseDirectory, "Entities", "Migrations");

            Stream stream = new FileStream(Path.Combine(basePath, "Domain.csv"), FileMode.Open, FileAccess.Read);

            CreateDomain(stream, context);

            stream = new FileStream(Path.Combine(basePath, "Role.csv"), FileMode.Open, FileAccess.Read);

            CreateRole(stream, context);

            stream = new FileStream(Path.Combine(basePath, "Person.csv"), FileMode.Open, FileAccess.Read);

            CreatePerson(stream, context);

            stream = new FileStream(Path.Combine(basePath, "User.csv"), FileMode.Open, FileAccess.Read);

            CreateUser(stream, context);

            stream = new FileStream(Path.Combine(basePath, "UserRoleMapping.csv"), FileMode.Open, FileAccess.Read);

            CreateUserRoleMapping(stream, context);

            stream = new FileStream(Path.Combine(basePath, "RoleFeatureMapping.csv"), FileMode.Open, FileAccess.Read);

            CreateRoleFeatureMapping(stream, context);
            stream = new FileStream(Path.Combine(basePath, "UserModelName.csv"), FileMode.Open, FileAccess.Read);

            CreateUserModelName(stream, context);


            stream = new FileStream(Path.Combine(basePath, "Metadata.csv"), FileMode.Open, FileAccess.Read);

            CreateMetadata(stream, context);

            stream = new FileStream(Path.Combine(basePath, "Feature.csv"), FileMode.Open, FileAccess.Read);

            CreateFeature(stream, context);

            stream = new FileStream(Path.Combine(basePath, "ApiKey.csv"), FileMode.Open, FileAccess.Read);

            CreateApiKey(stream, context);

            stream = new FileStream(Path.Combine(basePath, "Config.csv"), FileMode.Open, FileAccess.Read);

            CreateConfig(stream, context);

            stream = new FileStream(Path.Combine(basePath, "ApiConfig.csv"), FileMode.Open, FileAccess.Read);

            CreateApiConfig(stream, context);

            stream = new FileStream(Path.Combine(basePath, "PlanConfigDefaults.csv"), FileMode.Open, FileAccess.Read);

            CreatePlanConfigDefaults(stream, context);

            stream = new FileStream(Path.Combine(basePath, "DiscountCoupn.csv"), FileMode.Open, FileAccess.Read);

            CreateDiscoutCoupn(stream, context);


            stream = new FileStream(Path.Combine(basePath, "EmailContent.csv"), FileMode.Open, FileAccess.Read);

            CreateEmailContent(stream, context);

            stream = new FileStream(Path.Combine(basePath, "Link.csv"), FileMode.Open, FileAccess.Read);

            CreateLink(stream, context);
            stream = new FileStream(Path.Combine(basePath, "AribaCurrency.csv"), FileMode.Open, FileAccess.Read);
            CreateAribaCurrencyName(stream, context);


            stream = new FileStream(Path.Combine(basePath, "ContractCustomType.csv"), FileMode.Open, FileAccess.Read);
            CreateContractCustomType(stream, context);
            stream = new FileStream(Path.Combine(basePath, "CountryList.csv"), FileMode.Open, FileAccess.Read);
            CreateCountryList(stream, context);

            stream = new FileStream(Path.Combine(basePath, "UserAppMapping.csv"), FileMode.Open, FileAccess.Read);
            CreateUserAppMapping(stream, context);



        }

        public static void SaveEntities(RepositoryContext repositoryContext)
        {

            repositoryContext.OnBeforeSaving(Guid.Parse("DF78056A-1097-430C-B29A-0CC42E3ECE7B"));
            _ = repositoryContext.SaveChanges(true);
        }
    }
}