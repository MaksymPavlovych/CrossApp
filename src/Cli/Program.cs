using System.Runtime.InteropServices;

Console.OutputEncoding = System.Text.Encoding.UTF8; // щоб кирилиця не ламалась на Windows

Console.WriteLine("CrossApp – практикум з крос-платформного програмування");
Console.WriteLine("Студент: Павлович Максим, ФЕІ-34");
Console.WriteLine(new string('-', 52));
Console.WriteLine($"ОС (OSDescription)   : {RuntimeInformation.OSDescription}");
Console.WriteLine($"ОС (Environment)     : {Environment.OSVersion}");
Console.WriteLine($"Архітектура процесу  : {RuntimeInformation.ProcessArchitecture}");
Console.WriteLine($"Версія .NET (CLR)    : {Environment.Version}");
Console.WriteLine($"Runtime              : {RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"Каталог застосунку   : {AppContext.BaseDirectory}");
Console.WriteLine($"Поточний каталог     : {Environment.CurrentDirectory}");
Console.WriteLine(new string('-', 52));
Console.WriteLine("Предметна область: Склад (товари, партії, залишки, переміщення)");