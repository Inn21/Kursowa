using System.Collections.Generic;
using System.Linq;
using Core.Feature;
using Features.Tasks.Model;

namespace Features.Tasks
{
    public class TaskTypeFeature : BaseFeature
    {
        private List<TaskTypeDefinition> _taskTypes;

        public void Initialize()
        {
            CreateTaskTypeDefinitions();
        }


        public List<TaskTypeDefinition> GetAllTaskTypes()
        {
            return _taskTypes;
        }

        public TaskTypeDefinition GetDefinition(TaskType type)
        {
            return _taskTypes.FirstOrDefault(t => t.Type == type);
        }

        private void CreateTaskTypeDefinitions()
        {
            _taskTypes = new List<TaskTypeDefinition>
            {
                new TaskTypeDefinition
                {
                    Type = TaskType.PhysicalExercise,
                    Name = "Фізичні вправи",
                    Description = "Тренування, пробіжка, йога, розтяжка",
                    CompletionRewards = new List<RewardPoint> { new RewardPoint(1, RewardType.Strength), new RewardPoint(1, RewardType.Health) },
                    FailurePenalties = new List<RewardPoint> { new RewardPoint(-1, RewardType.Health) }
                },
                new TaskTypeDefinition
                {
                    Type = TaskType.Sleep,
                    Name = "Сон",
                    Description = "Нічний сон, денний сон",
                    CompletionRewards = new List<RewardPoint> { new RewardPoint(2, RewardType.Health) },
                    FailurePenalties = new List<RewardPoint> { new RewardPoint(-1, RewardType.Health), new RewardPoint(-1, RewardType.Strength) }
                },
                new TaskTypeDefinition
                {
                    Type = TaskType.Eating,
                    Name = "Прийом їжі",
                    Description = "Сніданок, обід, вечеря, здорові перекуси",
                    CompletionRewards = new List<RewardPoint> { new RewardPoint(1, RewardType.Health) },
                    FailurePenalties = new List<RewardPoint> { new RewardPoint(-1, RewardType.Strength) }
                },
                new TaskTypeDefinition
                {
                    Type = TaskType.WorkAndStudy,
                    Name = "Робота / Навчання",
                    Description = "Виконання професійних чи академічних завдань",
                    CompletionRewards = new List<RewardPoint> { new RewardPoint(1, RewardType.Intelligence) },
                    FailurePenalties = new List<RewardPoint> { new RewardPoint(-1, RewardType.Intelligence) }
                },
                new TaskTypeDefinition
                {
                    Type = TaskType.Housework,
                    Name = "Домашні справи / Похід по справах",
                    Description = "Прибирання, оплата рахунків, покупки",
                    CompletionRewards = new List<RewardPoint> { new RewardPoint(1, RewardType.Strength) },
                    FailurePenalties = new List<RewardPoint> { new RewardPoint(-1, RewardType.Health) }
                },
                new TaskTypeDefinition
                {
                    Type =TaskType.Creative,
                    Name = "Творчість / Хобі",
                    Description = "Читання, малювання, музика, рукоділля",
                    CompletionRewards = new List<RewardPoint> { new RewardPoint(1, RewardType.Intelligence) },
                    FailurePenalties = new List<RewardPoint> { new RewardPoint(-1, RewardType.Intelligence) }
                },
                new TaskTypeDefinition
                {
                    Type = TaskType.Hygiene,
                    Name = "Особиста гігієна",
                    Description = "Душ, гігієнічні процедури, візит до лікаря",
                    CompletionRewards = new List<RewardPoint> { new RewardPoint(1, RewardType.Health) },
                    FailurePenalties = new List<RewardPoint> { new RewardPoint(-1, RewardType.Health) }
                },
                new TaskTypeDefinition
                {
                    Type = TaskType.Rest,
                    Name = "Відпочинок / Вільний час",
                    Description = "Відпочинок, прогулянка, зустріч з друзями",
                    CompletionRewards = new List<RewardPoint> { new RewardPoint(1, RewardType.Health) },
                    FailurePenalties = new List<RewardPoint> { new RewardPoint(-1, RewardType.Health) }
                }
            };
        }
    }
}
