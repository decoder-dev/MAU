namespace NetworkMonitoringSystem.Core.Models
{
    /// <summary>
    /// Информация о Мурманском арктическом университете
    /// </summary>
    public static class UniversityInfo
    {
        public static string Name => "Мурманский арктический университет";
        public static string ShortName => "МАУ";
        public static string FullName => "Федеральное государственное автономное образовательное учреждение высшего образования «Мурманский арктический государственный университет»";

        public static string Website => "https://mauniver.ru/";
        public static string Email => "office@mauniver.ru";

        // Приёмная комиссия - Мурманск
        public static string AdmissionOfficeMurmanskAddress => "183010, г. Мурманск, пр. Кирова, д. 1, корпус Л, каб. 112";
        public static string AdmissionOfficeMurmanskPhone => "8 800 350 1221";
        public static string AdmissionOfficeMurmanskEmail => "priem@mauniver.ru";
        public static string AdmissionOfficeMurmanskHours => "пн-пт 09.00-17.00, перерыв 13.00-14.00, сб-вс выходной";

        // Приёмная комиссия - Апатиты
        public static string AdmissionOfficeApatityAddress => "184209, г. Апатиты, ул. Лесная, д. 29";
        public static string AdmissionOfficeApatityPhone => "8 815 556 6220, 8 964 687 0005";
        public static string AdmissionOfficeApatityEmail => "priem@arcticsu.ru";
        public static string AdmissionOfficeApatityHours => "пн-пт 9.00-17.30, сб-вс выходной";

        // Приёмная ректора
        public static string RectorOfficeAddress => "183038, г. Мурманск, ул. Капитана Егорова, д. 15, каб. 103";
        public static string RectorOfficePhone => "+7 (8152) 21-38-01";
        public static string RectorOfficeEmail => "office@mauniver.ru";

        // Социальные сети
        public static string VK => "https://vk.com/mauniver";
        public static string Telegram => "https://t.me/mauniver";
        public static string Rutube => "https://rutube.ru/mauniver";
        public static string Dzen => "https://dzen.ru/mauniver";

        // Получить полную информацию
        public static string GetFullInfo()
        {
            return $@"{FullName}

Приёмная комиссия (Мурманск):
{AdmissionOfficeMurmanskAddress}
Телефон: {AdmissionOfficeMurmanskPhone}
Email: {AdmissionOfficeMurmanskEmail}
Режим работы: {AdmissionOfficeMurmanskHours}

Приёмная комиссия (Апатиты):
{AdmissionOfficeApatityAddress}
Телефон: {AdmissionOfficeApatityPhone}
Email: {AdmissionOfficeApatityEmail}
Режим работы: {AdmissionOfficeApatityHours}

Приёмная ректора:
{RectorOfficeAddress}
Телефон: {RectorOfficePhone}
Email: {RectorOfficeEmail}

Сайт: {Website}";
        }
    }
}

