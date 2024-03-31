using System;
using System.Collections.Generic;
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
            var odataClient = new ODataClient(odataClientSettings);

            var searchTelegramUser = await odataClient.For("ITelegramUsers")
                .Filter(string.Format("TelegramUserId eq '{0}'", telegramUserId))
                .FindEntriesAsync();

            return searchTelegramUser == null ? false : true;
        }

        /// <summary>
        /// Создание простого документа
        /// </summary>
        /// <param name="telegramUserId"></param>
        /// <returns></returns>
        async public Task CreateSimpleDocument(long telegramUserId)
        {
            return;
        }
    }
}
