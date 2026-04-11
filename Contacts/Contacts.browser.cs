using Microsoft.Maui.Essentials;
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

                var results = JsonSerializer.Deserialize(result, AvaeJsonSerializerContext.Default.ListContactsResponseInterop);

                if (results is null)
                {
                    return contacts;
                }

                foreach (var item in results)
                {
                    var contact = new Contact()
                    {
                        GivenName = item.Name?.FirstOrDefault() ?? string.Empty,
                        Emails = item.Email?.Select(e => new ContactEmail { EmailAddress = e }).ToList() ?? [],
                        Phones = item.Tel?.Select(e => new ContactPhone { PhoneNumber = e }).ToList() ?? [],
                    };
                    contacts.Add(contact);
                }
                return contacts;
            }
            return [];
        }
    }
}
