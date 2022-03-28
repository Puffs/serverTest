using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TestServer
{
    // Класс-обработчик клиента
    class Client
    {
        protected internal NetworkStream Stream { get; private set; }

        public Client(TcpClient Client)
        {
            try
            {
                Stream = Client.GetStream();
                while (true)
                {

                    string answer; // Ответ клиенту
                    String message = GetMessage(); // Получаем строку от клиента

                    // Проверяем, является ли строка палиндромом
                    bool palindrom = isPalindrom(message);
                    if (palindrom)
                    {
                        answer = "Содержимое файла палиндромом";
                    }
                    else
                    {
                        answer = "Содержимое файла не палиндромом";
                    }

                    // Возвращаем ответ клиенту
                    byte[] msg = Encoding.Unicode.GetBytes(answer);
                    Console.WriteLine(answer);
                    Stream.Write(msg, 0, msg.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // Функция проверки строки на палиндром
        static bool isPalindrom(string text)
        {
            bool palindrom = true;
            char[] textChar = text.ToCharArray(); // преобразуем строку в массив символов
            int count = 0;
            // Сравниваем первый и последний элемент, второй и предпоследний и так далее
            while (textChar.Length / 2 > count)
            {
                palindrom = textChar[count].Equals(textChar[textChar.Length - count - 1]);
                count++;
                if (!palindrom) // если элементы не равны, выходим из цикла и возращаем false
                {
                    break;
                }
            }
            return palindrom;
        }

        // чтение входящего сообщения и преобразование в строку
        private string GetMessage()
        {
            byte[] data = new byte[64]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }
    }

    class Server
    {
        TcpListener Listener; // Объект, принимающий TCP-клиентов

        // Запуск сервера
        public Server(int Port)
        {
            Listener = new TcpListener(IPAddress.Any, Port); // Создаем "слушателя" для указанного порта
            Listener.Start(); // Запускаем его

            // В бесконечном цикле
            while (true)
            {

                // Принимаем новых клиентов. После того, как клиент был принят, он передается в новый поток (ClientThread)
                // с использованием пула потоков.
                ThreadPool.QueueUserWorkItem(new WaitCallback(ClientThread), Listener.AcceptTcpClient());

            }
        }

        static void ClientThread(Object StateInfo)
        {
            // Создаем новый экземпляр класса Client и передаем ему приведенный к классу TcpClient объект StateInfo
            new Client((TcpClient)StateInfo);
        }

        // Остановка сервера
        ~Server()
        {
            // Если "слушатель" был создан
            if (Listener != null)
            {
                // Остановим его
                Listener.Stop();
            }
        }

        static void Main(string[] args)
        {
            // Определим максимальное количество одновременно обрабатываемых потоков.
            int MaxThreadsCount;
            Console.WriteLine("Введите максимальное количество одновременно обрабатываемых потоков");
            // Проверка ввода числа в консоль
            while (true)
            {
                try
                {
                    MaxThreadsCount = Convert.ToInt32(Console.ReadLine());
                    // Проверка на положительное значение
                    if (MaxThreadsCount >= 2)
                    {
                        break; // выход из цикла, при соблюдении всех условий
                    }
                    else
                    {
                        Console.WriteLine("Введите положительное целое значение больше чем 0");
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Введите корректное число");
                }
            }
            Console.WriteLine("Cервер запущен");

            // Установим максимальное количество одновременно обрабатываемых потоков 
            ThreadPool.SetMaxThreads(MaxThreadsCount, MaxThreadsCount);
            // Установим минимальное количество рабочих потоков. 2 - главный поток и один клиент
            ThreadPool.SetMinThreads(2, 2);
            // Создадим новый сервер на порту 80
            new Server(80);

        }
    }
}
