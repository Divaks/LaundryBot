using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LaundryBot
{
    public class ProgramState
    {
        [JsonPropertyName("LeftSymbol")] public string LeftSymbol { get; set; }

        [JsonPropertyName("RightSymbol")] public string RightSymbol { get; set; }

        [JsonPropertyName("WorkingState")]
        public string WorkingState { get; set; }
    }

    public class WashingMachine
    {
        [JsonPropertyName("Number")] public int Number { get; set; }

        [JsonPropertyName("IsActive")] public bool IsActive { get; set; }

        [JsonPropertyName("ProgramState")] public ProgramState ProgramState { get; set; }

        public bool IsAvailable => IsActive && (ProgramState == null || ProgramState.WorkingState == "готово" ||
                                                ProgramState.WorkingState == "завершено");

        public string GetStatusInfo()
        {
            if (!IsActive) return "Вимкнена";
            if (IsAvailable) return "Вільна";
            if (ProgramState == null) return "Зайнята, статус невідомий";

            if (int.TryParse(ProgramState.LeftSymbol, out int left) &&
                int.TryParse(ProgramState.RightSymbol, out int right))
            {
                int timeInMinutes = left * 10 + right;
                if (timeInMinutes > 0)
                {
                    return $"Зайнята ({ProgramState.WorkingState}), залишилось *{timeInMinutes} хв.*";
                }
            }

            return $"Зайнята ({ProgramState.WorkingState})";
        }
    }

    public class TerminalState
    {
        [JsonPropertyName("CodeName")] public string CodeName { get; set; }

        [JsonPropertyName("WMs")] public List<WashingMachine> WMs { get; set; }
    }
}