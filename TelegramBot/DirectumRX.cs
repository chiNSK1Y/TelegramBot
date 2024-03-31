using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Simple.OData.Client;

namespace TelegramBot
{
    internal class DirectumRX
    {
        // URL сервиса интеграции.
        private const string IntegrationServiceUrl = "http://tanaisdirrx4_razr_test/DrxIntegrationLocal/odata/"; 
        // Логин для Basic-аутентификации.
        private const string Login = "Administrator";
        // Пароль для Basic-аутентификации.
        private const string Password = "11111";
        //Параметры подключения
        static private ODataClientSettings odataClientSettings;

        //Конструктор
        public DirectumRX()
        {
            // Настройки Simple OData Client: добавление ко всем запросам URL сервиса и
            // заголовка с данными аутентификации.
            var odataSettings = new ODataClientSettings(new Uri(IntegrationServiceUrl));
            odataSettings.BeforeRequest += (HttpRequestMessage message) =>
            {
                var authenticationHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Login}:{Password}"));
                message.Headers.Add("Authorization", "Basic " + authenticationHeaderValue);
            };
            odataClientSettings = odataSettings;
        }

        /// <summary>
        /// Регистрация пользователя в DirectumRX
        /// </summary>
        /// <param name="telegramUserId">ИД пользователя в Telegram</param>
        /// <returns>true регистрация завершилась успешно, false была ошибка при регистрации</returns>
        async public Task<bool> RegistrationUser(long telegramUserId)
        {
            try
            {
                var odataClient = new ODataClient(odataClientSettings);

                // Параметры.
                var newEmployeeProperties = new
                {
                    Person = new { Id = 1643 },
                    Status = "Active",
                    TelegramUserId = telegramUserId
                };

                var newTelegramUser = await odataClient.For("ITelegramUsers")
                  .Set(newEmployeeProperties)
                  .InsertEntryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Проверить наличие пользователя в DirectumRX
        /// </summary>
        /// <param name="telegramUserId">ИД пользователя в Telegram</param>
        /// <returns>true пользователь есть, false пользователя нет</returns>
        async public Task<bool> CheckRegisteredUser(long telegramUserId)
        {
            /*
            var odataClient = new ODataClient(odataClientSettings);

            var searchTelegramUser = await odataClient.For("ITelegramUsers")
                .Filter(string.Format("TelegramUserId eq '{0}'", telegramUserId))
                .FindEntriesAsync();

            return searchTelegramUser == null ? false : true;
            */
            return true;
        }

        /// <summary>
        /// Создание простого документа
        /// </summary>
        /// <param name="telegramUserId"></param>
        /// <returns></returns>
        async public Task<long> CreateSimpleDocument(long telegramUserId, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return 0;

            // Получение наименования файла без расширения
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            // Получение расширения файла
            string extension = Path.GetExtension(filePath).ToLower(); 

            // Чтение содержимого файла в байтах
            byte[] fileBytes = File.ReadAllBytes(filePath);
            // Кодирование в Base64
            string base64String = Convert.ToBase64String(fileBytes); 

            var odataClient = new ODataClient(odataClientSettings);

            // Создание документа типа SimpleDocument.
            var newSimpleDocument = await odataClient.For("ISimpleDocuments")
              .Set(new { Name = fileName })
              .InsertEntryAsync();

            // Поиск идентификатора подходящего приложения-обработчика для документа.
            var associatedApplication = await odataClient.For("IAssociatedApplications")
              .Filter($"Extension eq '{extension}'")
              .FindEntriesAsync();
            var associatedApplicationId = associatedApplication.First()["Id"];

            // Создание версии документа.
            var newVersion = await odataClient.For("ISimpleDocuments").Key(newSimpleDocument["Id"])
              .NavigateTo("Versions")
              .Set(new { Number = 1, AssociatedApplication = new { Id = associatedApplicationId } })
              .InsertEntryAsync();

            // Добавление строки, закодированной в Base64, в свойство Body версии
            // документа. Для примера взята строка "111111".
            await odataClient.For("ISimpleDocuments ").Key(newSimpleDocument["Id"])
              .NavigateTo("Versions").Key(newVersion["Id"])
              .NavigateTo("Body")
              .Set(new { Value = base64String })
              .InsertEntryAsync();

            //Удаление файла
            File.Delete(filePath);

            return (long)newSimpleDocument["Id"];
        }
    }
}
