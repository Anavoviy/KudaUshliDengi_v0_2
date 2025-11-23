using Telegram.Bot.Types.ReplyMarkups;

namespace KudaUshliDengi_v0_2.application.telegram.builders;

public static class KeyboardBuilder
{
    public static ReplyKeyboardMarkup Main
        => new ReplyKeyboardMarkup(resizeKeyboard: true)
            .AddButtons("Цели", "Лимиты", "Категории").AddNewRow()
            .AddButtons("Помощь", "Отчеты", "Ещё");
    public static ReplyKeyboardMarkup More
        => new ReplyKeyboardMarkup(resizeKeyboard: true)
            .AddButtons("Отмена", "Исправить").AddNewRow()
            .AddButtons("Назад");

    public static ReplyKeyboardMarkup Categories
        => new ReplyKeyboardMarkup(resizeKeyboard: true)
            .AddButtons("Все категории", "Назад");

    public static ReplyKeyboardMarkup Goals
        => new ReplyKeyboardMarkup(resizeKeyboard: true)
            .AddButtons("Все цели", "Назад");
    public static ReplyKeyboardMarkup Limits
        => new ReplyKeyboardMarkup(resizeKeyboard: true)
            .AddButtons("Бюджет").AddNewRow()
            .AddButton("Назад");

    public static ReplyMarkup? Reports 
        => new ReplyKeyboardMarkup(resizeKeyboard: true)
            .AddButtons("отчет за неделю", "отчет за месяц").AddNewRow()
            .AddButtons("отчет за квартал", "отчет за полгода").AddNewRow()
            .AddButton("отчет за год")
            .AddNewRow().AddButton("Назад");
}