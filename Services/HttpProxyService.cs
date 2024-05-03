
using CommunityToolkit.Maui.Alerts;
using System.Net.Http.Json;
using System.Text.Json;

namespace SukaliCareApp.Services
{
    public class HttpProxyService
    {
        private readonly HttpClient _httpClient;
        public HttpProxyService()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(Stores.BaseUrl)};
        }
        public async Task<string> GetUserDetails(string email)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/clients/record?email={email}");
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    ClientDTO result = await response.Content.ReadFromJsonAsync<ClientDTO>();
                    AppUser user = new()
                    {
                        Email = result.Email,
                        FirstName = result.FirstName,
                        LastName = result.LastName,
                        Phone = result.Phone,
                        DoB = result.DoB,
                        MiddleName = result.MiddleName,
                        ProfilePhoto = result.ProfilePhoto,
                        Id = result.Id,
                    };
                    Preferences.Set(Stores.userDetailsStore, JsonSerializer.Serialize(user));
                    Preferences.Set(Stores.completedAccountStore, true);

                    return "success";
                }
                else if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return "failed";
                }
                return "error";
            }
            catch (Exception)
            {
                return "error";
            }
        }

        public async Task<bool> PostUserDetails(AppUser user)
        {
            try
            {
                var result = await _httpClient.PostAsJsonAsync("api/clients/create", new ClientDTO
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    MiddleName = user.MiddleName,
                    LastSeenOn = DateTime.Now,
                    DoB = user.DoB,
                    Phone = user.Phone,
                    RegisteredOn = DateTime.Now
                });
                if(result.IsSuccessStatusCode)
                {
                    dynamic response = await result.Content.ReadFromJsonAsync<dynamic>();
                    user.Id = response.Id;
                    Preferences.Set(Stores.userDetailsStore, JsonSerializer.Serialize(user));
                    Preferences.Set(Stores.completedAccountStore, true);
                }
                return result.IsSuccessStatusCode;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdatePreference(UserPreferences preferences)
        {
            try
            {
                string email = Preferences.Get(Stores.firebaseUserAccountStore, null);
                var result = await _httpClient.PostAsJsonAsync($"preferences/update?email={email}", preferences);
                if (result.IsSuccessStatusCode)
                    return true;

                return false;
            }
            catch 
            { 
                return false;
            }
        }

        public async Task<bool> UploadReadings(IEnumerable<CgmReading> reading)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "api/readings/post", reading.Select(item => new GlucoseReadingDTO
                    {
                        GlucoseLevel = item.GlucoseValue.Value,
                        ReadingUnits = item.GlucoseValue.Unit,
                        ReadingDateTime = item.ReadingDate,
                        ReadingId = item.Id
                    }));

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<ReadingEntity>> GetReadings(string email)
        {
            try
            {
                var results = await _httpClient.GetFromJsonAsync<IEnumerable<GlucoseReadingDTO>>("api/readings/all") ?? [];

                return results
                    .Where(x => x.ReadingId != Guid.Empty)
                    .Select(item => new ReadingEntity
                    {
                        Id = item.ReadingId,
                        Value = item.GlucoseLevel,
                        Unit = item.ReadingUnits,
                        ReadingDate = item.ReadingDateTime,
                        UserId = item.ClientId.ToString(),
                        UserName = item.ClientId.ToString()
                    }).ToList();
            }
            catch
            {
                return [];
            }
        }
        public async Task<bool> SaveDevice(CGMDevice device)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/devices/register", new CgmDevicesDTO
                {
                    DeviceName = device.DeviceName,
                    SerialNumber = device.Uuid,
                    Uuid = device.Uuid,
                    LastConnected = DateTime.Now,
                    MacAddress = device.Uuid,
                });

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }

        }
        public async Task<UserPreferences> GetPreferences()
        {
            try
            {
                string email = Preferences.Get(Stores.firebaseUserAccountStore, null);
                dynamic result = await _httpClient.GetFromJsonAsync<dynamic>($"preferences?email={email}");
                UserPreferences prefs = new UserPreferences
                {
                    AllowAutoConnect = result.AllowAutoConnect,
                    SyncFrequency = result.SyncFrequency,
                    AllowNotifications = result.AllowNotification,
                    Units = result.Units
                };
                Preferences.Set(Stores.preferenceStore, JsonSerializer.Serialize(prefs));
                return prefs;
            }
            catch
            {
                return null;
            }
        }

        public async Task<IEnumerable<LibResource>> GetLibResourcesAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<LibResourceDTO[]>("api/library/all") ?? throw new Exception();
                return result.Select(item => new LibResource
                {
                    Id = item.Id,
                    Author = item.Author,
                    Description = item.Description,
                    PublishedOn = item.PublishedOn,
                    Thumbnail = item.Thumbnail,
                    Title = item.Thumbnail,
                    Url = item.Url
                });
            }
            catch (Exception)
            {
                return new List<LibResource>()
                {
                    new LibResource
                        {
                            Title = "Understanding Type 2 Diabetes",
                            Description = "Comprehensive guide to Type 2 diabetes, covering causes, symptoms, and effective management strategies. Arm yourself with knowledge for a healthier lifestyle.",
                            Url = "https://essentialhh.org/wp-content/uploads/2021/06/509_main.jpg",
                            Thumbnail = "https://blogs.cdc.gov/wp-content/uploads/sites/6/2020/11/PHM_Diabetes-Month_header.jpg",
                            PublishedOn = DateTime.Parse("2023-04-05"),
                            Author = "Health Insights Team"
                        },
                        new LibResource
                        {
                            Title = "Diabetes and Heart Health Connection",
                            Description = "Explore the intricate link between diabetes and heart health. Learn preventive measures and lifestyle changes to safeguard your cardiovascular well-being.",
                            Url = "https://essentialhh.org/wp-content/uploads/2021/06/509_main.jpg",
                            Thumbnail = "https://essentialhh.org/wp-content/uploads/2021/06/509_main.jpg",
                            PublishedOn = DateTime.Parse("2023-05-20"),
                            Author = "CardioCare Experts"
                        },
                        new LibResource
                        {
                            Title = "Insulin Injection Techniques",
                            Description = "Master proper insulin injection techniques for optimal diabetes management. This guide provides step-by-step instructions and tips for a pain-free experience.",
                            Url = "https://www.biospectrumasia.com/uploads/articles/1-15306.jpg",
                            Thumbnail = "https://www.biospectrumasia.com/uploads/articles/1-15306.jpg",
                            PublishedOn = DateTime.Parse("2023-06-12"),
                            Author = "Endocrine Wellness Team"
                        },
                        new LibResource
                        {
                            Title = "Living Well with Gestational Diabetes",
                            Description = "A supportive resource for pregnant individuals with gestational diabetes. Find practical advice, nutritional guidance, and emotional support for a healthy pregnancy.",
                            Url = "https://media.istockphoto.com/photos/world-diabetes-day-and-healthcare-concept-patients-blood-sugar-and-picture-id1224566625?b=1&k=6&m=1224566625&s=170667a&w=0&h=KM7CZPvbmUvghN3OGfWbccpCRvFqnCYKZad4uNhCajc=",
                            Thumbnail = "https://blog-images-1.pharmeasy.in/2020/01/26182504/041.jpg",
                            PublishedOn = DateTime.Parse("2023-07-08"),
                            Author = "Maternal Health Experts"
                        },
                        new LibResource
                        {
                            Title = "Diabetes and Exercise: A Comprehensive Guide",
                            Description = "Unlock the benefits of exercise in diabetes management. Discover tailored workout routines, safety precautions, and motivational tips for an active lifestyle.",
                            Url = "https://www.family.abbott/in-en/ensure/guides-to-healthy-living/take-control-of-your-diabetes/manage-your-diabetes-with-the-four-pillars/_jcr_content/root/container/columncontrol/tab_item_no_1/image_copy_copy.coreimg.85.1280.jpeg/1669365934800/image-fh.jpeg",
                            Thumbnail = "https://www.family.abbott/in-en/ensure/guides-to-healthy-living/take-control-of-your-diabetes/manage-your-diabetes-with-the-four-pillars/_jcr_content/root/container/columncontrol/tab_item_no_1/image_copy_copy.coreimg.85.1280.jpeg/1669365934800/image-fh.jpeg",
                            PublishedOn = DateTime.Parse("2023-08-22"),
                            Author = "Fitness Wellness Team"
                        },
                        new LibResource
                        {
                            Title = "Emotional Well-being in Diabetes",
                            Description = "Address the emotional aspects of living with diabetes. Learn coping strategies, stress management techniques, and the importance of mental health in your diabetes journey.",
                            Url = "https://edhub.ama-assn.org/ImageLibrary/CourseImages/221/diabetes-720x405.jpg",
                            Thumbnail = "https://edhub.ama-assn.org/ImageLibrary/CourseImages/221/diabetes-720x405.jpg",
                            PublishedOn = DateTime.Parse("2023-09-18"),
                            Author = "Psychological Wellness Experts"
                        }

                };
            }
        }
        private bool CheckConnectivity() => Connectivity.NetworkAccess == NetworkAccess.Internet;
    }
}
