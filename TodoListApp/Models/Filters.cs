﻿namespace TodoListApp.Models
{
    public class Filters
    {
        public Filters(string filterString) {
            FilterString = filterString ?? "0-all-0-0";
            string[] filters = FilterString.Split('-');
            CategoryId = filters[0];
            Due = filters[1];
            PriorityId = filters[2];
            StatusId = filters[3];
        }

        public string FilterString { get; }
        public string CategoryId { get; }
        public string Due { get; }
        public string PriorityId { get; }
        public string StatusId { get; }

        public bool HasCategory => int.Parse(CategoryId) != 0;
        public bool HasDue => Due.ToLower() != "all";
        public bool HasPriority => int.Parse(PriorityId) != 0;
        public bool HasStatus => int.Parse(StatusId) != 0;

        public static Dictionary<string, string> DueFilterValues =>
            new Dictionary<string, string>
            {
                {"future","Future"},
                {"past","Past"},
                {"today","Today"}
        };

        public bool IsPast => Due.ToLower() == "past";
        public bool IsFuture => Due.ToLower() == "future";
        public bool IsToday => Due.ToLower() == "today";
    }
}
