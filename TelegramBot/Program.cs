using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot;

namespace TelegramBot
{
    internal class Program
    {
        // Это клиент для работы с Telegram Bot API, который позволяет отправлять сообщения, управлять ботом, подписываться на обновления и многое другое.
        private static ITelegramBotClient botClient;

        // Это объект с настройками работы бота. Здесь мы будем указывать, какие типы Update мы будем получать, Timeout бота и так далее.
        private static ReceiverOptions receiverOptions;

        //Ключ Api
        const string apiTelegramKey = "7120360775:AAEouZykT2AhZclMFTjwChWMK6pMwmiXGRA";

        static async Task Main()
        {
            botClient = new TelegramBotClient(apiTelegramKey);
            receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[]
                {
                    UpdateType.Message,
                    UpdateType.CallbackQuery
                },
                // Параметр, отвечающий за обработку сообщений, пришедших за то время, когда ваш бот был оффлайн
                // True - не обрабатывать, False (стоит по умолчанию) - обрабаывать
                ThrowPendingUpdates = true,
            };

            using (var cts = new CancellationTokenSource())
            {
                botClient.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions, cts.Token);

                var me = await botClient.GetMeAsync(); // Создаем переменную, в которую помещаем информацию о нашем боте

                Console.WriteLine($"{me.FirstName} запущен!");

                await Task.Delay(-1); // Устанавливаем бесконечную задержку, чтобы наш бот работал постоянно
            }
        }

        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Обязательно ставим блок try-catch, чтобы наш бот не "падал" в случае каких-либо ошибок
            try
            {
                // Сразу же ставим конструкцию switch, чтобы обрабатывать приходящие Update
                switch (update.Type)
                {
                    //Обработчик сообщений (команд)
                    case UpdateType.Message:
                        {
                            //Console.WriteLine("Пришло сообщение!");
                            await UpdateMessageHandler(botClient, update, cancellationToken);
                            return;
                        }

                    //Обработчик кнопок
                    case UpdateType.CallbackQuery:
                        {
                            await UpdateCallbackQueryHandler(botClient, update, cancellationToken);
                            return;
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            // Тут создадим переменную, в которую поместим код ошибки и её сообщение 
            string ErrorMessage = string.Empty;
            if (error is ApiRequestException)
                ErrorMessage = string.Format("Telegram API Error:\n {0} \n {1}", ((ApiRequestException)error).ErrorCode, error.Message);
            else
                ErrorMessage = string.Format("Telegram API Error:\n {0}", error.Message);

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        //Обработчик для типа сообщения Message
        private static async Task UpdateMessageHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            //Сообщение
            var message = update.Message;
            //От кого
            var userFrom = message.From;

            // Chat - содержит всю информацию о чате
            var chat = message.Chat;

            //На консоле отобразим что пришло и от кого
            Console.WriteLine($"{userFrom.FirstName} ({userFrom.Id}) написал сообщение: {message.Text}");

            //Команда help
            if (!string.IsNullOrEmpty(message.Text) && message.Text.StartsWith("/"))
            {
                if (message.Text == "/help")
                {
                    await botClient.SendTextMessageAsync(
                                chat.Id,
                                "/start - запуск\n/help - список команд",
                                replyToMessageId: message.MessageId // по желанию можем поставить этот параметр, отвечающий за "ответ" на сообщение
                            );
                    return;
                }

                //Команда /start
                if (message.Text == "/start")
                {
                    //Интеграция с directum rx
                    DirectumRX directumRX = new DirectumRX();

                    //Проверяем наличие пользака в Directum RX
                    bool isUserFound = await directumRX.CheckRegisteredUser(update.Message.From.Id);

                    //Если есть, то показывае одни кнопки, иначе другие
                    if (isUserFound)
                    {
                        // Тут создаем нашу клавиатуру
                        var inlineKeyboard = new InlineKeyboardMarkup(
                            new List<InlineKeyboardButton[]>() // здесь создаем лист (массив), который содрежит в себе массив из класса кнопок
                            {
                                        // Каждый новый массив - это дополнительные строки,
                                        // а каждая дополнительная строка (кнопка) в массиве - это добавление ряда

                                        new InlineKeyboardButton[] // тут создаем массив кнопок
                                        {
                                            InlineKeyboardButton.WithUrl("Справка", "https://habr.com/"), //TODO показать потом справку
                                            InlineKeyboardButton.WithCallbackData("Какое-то действие", "action"),
                                        },
                            });

                        await botClient.SendTextMessageAsync(
                                    chat.Id,
                                    "Панель",
                                    replyMarkup: inlineKeyboard);
                    }
                    //Показываем кнопку с регистрацией
                    else
                    {
                        // Тут создаем нашу клавиатуру
                        var inlineKeyboard = new InlineKeyboardMarkup(
                            new List<InlineKeyboardButton[]>() // здесь создаем лист (массив), который содрежит в себе массив из класса кнопок
                            {
                                        // Каждый новый массив - это дополнительные строки,
                                        // а каждая дополнительная строка (кнопка) в массиве - это добавление ряда

                                        new InlineKeyboardButton[] // тут создаем массив кнопок
                                        {
                                            InlineKeyboardButton.WithUrl("Справка", "https://habr.com/"), //TODO показать потом справку
                                            InlineKeyboardButton.WithCallbackData("Зарегистрироваться", "registration"),
                                        },
                            });

                        await botClient.SendTextMessageAsync(
                                    chat.Id,
                                    "Регистрация",
                                    replyMarkup: inlineKeyboard);
                    }
                    return;
                }

                //Возвращаем что команда не определена
                await botClient.SendTextMessageAsync(
                                chat.Id,
                                "Не опознанная команда",
                                replyToMessageId: message.MessageId // по желанию можем поставить этот параметр, отвечающий за "ответ" на сообщение
                            );
                return;
            }
        }

        private static async Task UpdateCallbackQueryHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.CallbackQuery == null)
                return;

            //Сообщение
            var message = update.CallbackQuery.Message;

            //Интеграция с directum rx
            DirectumRX directumRX = new DirectumRX();

            //Регистрация
            if (update.CallbackQuery.Data == "registration")
            {
                //Проверяем наличие пользователя
                bool isRegistered = await directumRX.RegistrationUser(message.From.Id);

                if (isRegistered)
                {
                    //Отправляем ответ что регистрация завершена и нужно выполнить команду /start
                    await botClient.SendTextMessageAsync(
                                    message.Chat.Id,
                                    "Регистрация завершена, выполните команду /start для просмотра доступны действий.",
                                    replyToMessageId: message.MessageId // по желанию можем поставить этот параметр, отвечающий за "ответ" на сообщение
                                );
                }
                else
                {
                    //Отправляем ответ что регистрация завершена и нужно выполнить команду /start
                    await botClient.SendTextMessageAsync(
                                    message.Chat.Id,
                                    "Регистрация завершилась с ошибкой, попробуйте позже!",
                                    replyToMessageId: message.MessageId // по желанию можем поставить этот параметр, отвечающий за "ответ" на сообщение
                                );
                }
                
                return;
            }

            return;
        }
    }
}
