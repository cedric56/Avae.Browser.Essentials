using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;

namespace Microsoft.Maui.ApplicationModel.Communication
{
    partial class ContactsImplementation : IContacts
    {
        [JSImport("contactsInterop.getAllAsync", "essentials")]
        public static partial Task<string> GetAll(bool multiple);

        public async Task<IEnumerable<Contact>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var result = await GetAll(true);
            return Parse(result);
        }

        public async Task<Contact?> PickContactAsync()
        {
            var result = await GetAll(false);
            return Parse(result).FirstOrDefault();
        }

        private static List<Contact> Parse(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                var contacts = new List<Contact>();

                //var results = JsonConvert.DeserializeObject<List<Root>>(result);
                var results = JsonSerializer.Deserialize<List<Root>>(result);

                
                foreach (var item in results ?? [])
                {
                    var contact = new Contact()
                    {
                        GivenName = item.name.FirstOrDefault(),
                        Emails = [.. item.email.Select(e => new ContactEmail { EmailAddress = e })],
                        Phones = [.. item.tel.Select(e => new ContactPhone { PhoneNumber = e })],
                    };
                    contacts.Add(contact);
                }
                return contacts;
            }
            return [];
        }

        public class Root
        {
            public List<string> name { get; set; }
            public List<string> email { get; set; }            
            public List<string> tel { get; set; }
            public List<string> address { get; set; }
        }
    }
}
