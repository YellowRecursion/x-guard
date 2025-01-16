using System.Diagnostics;
using XGuardLibrary;
using XGuardLibrary.Utilities;

namespace XGuardLauncher
{
    internal class Program
    {
        private static readonly string _appName = "XGuardMain";
        private static readonly string _appDir = AppDomain.CurrentDomain.BaseDirectory;

        public static ProgramData? Data { get; private set; }

        static void Main(string[] args)
        {
            Data = new ProgramData();

            if (Data.IsFileExists()) 
            {
                Data.Load();
            }

            MainMenu();
        }

        private static void MainMenu()
        {
            Console.Clear();

            Console.WriteLine("Привет!");
            Console.WriteLine("Это - XGuard");
            Console.WriteLine("Наш Discord: https://discord.gg/tNYwC6mDP2");

            Separator();

            if (Process.GetProcessesByName(_appName).Length > 0)
            {
                Console.WriteLine("XGuard уже работает");

                Separator();

                var option = Menu("Остановить программу", "Выйти");

                if (option == 0)
                {
                    Console.Write("Введите токен завершения работы: ");
                    var token = Console.ReadLine();
                    if (HashingUtility.ComputeSHA256Hash(token) == Data.TerminationTokenHash)
                    {
                        Console.WriteLine("Завершаем работу программы...");

                        if (File.Exists(Path.Combine(_appDir, "termination.txt")))
                        {
                            File.Delete(Path.Combine(_appDir, "termination.txt"));
                            Thread.Sleep(1000);
                        }

                        File.WriteAllText(Path.Combine(_appDir, "termination.txt"), token);

                        Thread.Sleep(3000);

                        if (File.Exists(Path.Combine(_appDir, "termination.txt")))
                        {
                            File.Delete(Path.Combine(_appDir, "termination.txt"));
                        }

                        TaskSchedulerUtilities.RemoveTask(_appName);

                        Console.WriteLine("XGuard завершил работу");
                        Thread.Sleep(1000);
                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.WriteLine("Введённый токен не верный");
                        Console.WriteLine("Возврат к главному меню...");
                        Thread.Sleep(2000);
                        MainMenu();
                        return;
                    }
                }
                if (option == 1)
                {
                    Environment.Exit(0);
                }
            }
            else
            {
                Console.WriteLine("XGuard ещё не запущен");

                Separator();

                if (TaskSchedulerUtilities.HasTask(_appName))
                {
                    Console.WriteLine("Программа автоматически запустится после перезапуска компьютера");

                    Separator();

                    var option = Menu("Выйти");

                    if (option == 0)
                    {
                        Environment.Exit(0);
                    }
                }
                else
                {
                    var option = Menu("Настроить и запустить программу", "Выйти");

                    if (option == 0)
                    {
                        Separator();

                        Data = new ProgramData();

                        Console.WriteLine("Для начала создайте Telegram бота, с помощью него можно будет управлять программой.");
                        Console.Write("Как бот будет готов, напишите сюда его токен: ");
                        Data.BotToken = Console.ReadLine();

                        HashSet<long> moderatorUserIds = new HashSet<long>();
                        string input;

                        Console.WriteLine("Введите идентификаторы пользователей-модераторов (введите 'exit' для завершения ввода):");

                        while (true)
                        {
                            Console.Write("Введите ID: ");
                            input = Console.ReadLine();

                            if (input.ToLower() == "exit")
                            {
                                break; // Завершение ввода
                            }

                            if (long.TryParse(input, out long userId))
                            {
                                moderatorUserIds.Add(userId); // Добавляем ID в коллекцию
                                Console.WriteLine($"ID {userId} добавлен.");
                            }
                            else
                            {
                                Console.WriteLine("Неверный ввод. Пожалуйста, введите корректный числовой ID или 'exit' для завершения.");
                            }
                        }

                        // Пример вывода всех введённых ID
                        Console.WriteLine("Список модераторов:");
                        foreach (var id in moderatorUserIds)
                        {
                            Console.WriteLine(id);
                        }

                        Data.ModeratorUserIds = moderatorUserIds;

                        Console.WriteLine("Теперь введите токен для остановки программы. Без него программу нельзя остановить.");
                        Console.Write("Запишите этот токен и сохраните его в надежном месте: ");
                        string terminationToken = Console.ReadLine();

                        // Валидация токена
                        while (string.IsNullOrWhiteSpace(terminationToken))
                        {
                            Console.WriteLine("Токен не может быть пустым. Пожалуйста, введите токен еще раз:");
                            terminationToken = Console.ReadLine();
                        }

                        // Запрос подтверждения
                        Console.WriteLine("Вы точно записали токен? Если да, напишите 'yes':");
                        string confirmation = Console.ReadLine();

                        while (!confirmation.Equals("yes", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("Пожалуйста, подтвердите, что вы записали токен. Напишите 'yes' для подтверждения:");
                            confirmation = Console.ReadLine();
                        }

                        Data.TerminationTokenHash = HashingUtility.ComputeSHA256Hash(terminationToken);

                        Data.Save();

                        TaskSchedulerUtilities.RegisterSystemTask(_appName, Path.Combine(_appDir, _appName + ".exe"));

                        Console.WriteLine("Всё готово! Перезагрузите компьютер для запуска программы");

                        Console.ReadKey();
                    }
                    if (option == 1)
                    {
                        Environment.Exit(0);
                    }
                }
            }
        }

        private static void Separator()
        {
            Console.WriteLine("\n---------------------------------------------\n");
        }

        private static void PressAnyKeyToExit()
        {
            Console.WriteLine("\n________________________________________________\n");
        }

        private static int Menu(params string[] options)
        {
            if (options == null || options.Length == 0)
            {
                throw new ArgumentException("Options cannot be null or empty.", nameof(options));
            }

            int selectedOption = -1;

            for (int i = 0; i < options.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {options[i]}");
            }

            Console.Write("> ");

            while (selectedOption < 0 || selectedOption >= options.Length)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (char.IsDigit(keyInfo.KeyChar))
                {
                    int input = int.Parse(keyInfo.KeyChar.ToString());
                    if (input > 0 && input <= options.Length)
                    {
                        selectedOption = input - 1; // Приводим к индексу массива
                    }
                }
            }

            return selectedOption;
        }
    }
}
