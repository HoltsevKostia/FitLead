using FitLead.Domain.Trainings.Exercises;
using Microsoft.EntityFrameworkCore;

namespace FitLead.Infrastructure.Persistence.Seeding
{
    public static class PlatformExerciseSeeder
    {
        public static IReadOnlyList<PlatformExerciseSeedItem> Exercises { get; } =
        [
            new("Присідання з власною вагою", "Поставте стопи приблизно на ширині плечей, опускайте таз назад і вниз, зберігаючи рівну спину та контроль колін.", MuscleGroup.Legs, Equipment.Bodyweight),
            new("Випади вперед", "Зробіть крок вперед, опустіть заднє коліно донизу та поверніться у вихідне положення, тримаючи корпус стабільним.", MuscleGroup.Legs, Equipment.Bodyweight),
            new("Випади назад", "Крокуйте назад, опускайтесь контрольовано та поверніться у стійку, не завалюючи коліно передньої ноги всередину.", MuscleGroup.Legs, Equipment.Bodyweight),
            new("Ягодичний міст", "Ляжте на спину, зігніть ноги та піднімайте таз вгору, напружуючи сідниці у верхній точці.", MuscleGroup.Glutes, Equipment.Bodyweight),
            new("Підйом на носки стоячи", "Піднімайтесь на носки з повною амплітудою, коротко затримуйтесь угорі та повільно опускайтесь.", MuscleGroup.Legs, Equipment.Bodyweight),
            new("Планка", "Утримуйте тіло в прямій лінії на передпліччях або долонях, не провалюючи поперек і не піднімаючи таз занадто високо.", MuscleGroup.Core, Equipment.Bodyweight),
            new("Бічна планка", "Утримуйте корпус на одному передпліччі, зберігаючи пряму лінію від плеча до стоп.", MuscleGroup.Core, Equipment.Bodyweight),
            new("Скручування на прес", "Ляжте на спину та піднімайте верхню частину корпуса за рахунок м'язів живота, не тягнучи шию руками.", MuscleGroup.Core, Equipment.Bodyweight),
            new("Підйом ніг лежачи", "Лежачи на спині, піднімайте прямі або трохи зігнуті ноги, контролюючи поперек і темп руху.", MuscleGroup.Core, Equipment.Bodyweight),
            new("Віджимання від підлоги", "Опускайте корпус до підлоги та повертайтесь вгору, тримаючи тіло в одній лінії.", MuscleGroup.Chest, Equipment.Bodyweight),
            new("Віджимання з колін", "Виконуйте віджимання з опорою на коліна, зберігаючи контроль корпуса та плечей.", MuscleGroup.Chest, Equipment.Bodyweight),
            new("Жим гантелей лежачи", "Лежачи на лаві, вичавлюйте гантелі вгору над грудьми та повільно опускайте їх до комфортної амплітуди.", MuscleGroup.Chest, Equipment.Dumbbells),
            new("Жим штанги лежачи", "Лежачи на лаві, опускайте штангу до грудей і вичавлюйте її вгору, контролюючи траєкторію та положення лопаток.", MuscleGroup.Chest, Equipment.Barbell),
            new("Жим гантелей над головою", "Вичавлюйте гантелі вгору над плечима, тримаючи корпус стабільним і не прогинаючи поперек.", MuscleGroup.Shoulders, Equipment.Dumbbells),
            new("Тяга гантелі в нахилі", "Спираючись однією рукою або утримуючи нахил корпуса, тягніть гантель до таза, зводячи лопатку.", MuscleGroup.Back, Equipment.Dumbbells),
            new("Тяга верхнього блока", "Тягніть рукоять до верхньої частини грудей, опускаючи лопатки та не розгойдуючи корпус.", MuscleGroup.Back, Equipment.Cable),
            new("Горизонтальна тяга блока", "Тягніть рукоять до корпуса, зберігаючи рівну спину та контрольований рух лопаток.", MuscleGroup.Back, Equipment.Cable),
            new("Підтягування", "Підтягуйтесь до перекладини, починаючи рух зі спини та уникаючи неконтрольованого розгойдування.", MuscleGroup.Back, Equipment.PullUpBar),
            new("Румунська тяга", "Відводьте таз назад і опускайте вагу вздовж ніг, зберігаючи нейтральну спину та легкий згин у колінах.", MuscleGroup.Glutes, Equipment.Barbell),
            new("Станова тяга", "Піднімайте вагу з підлоги за рахунок ніг і спини, тримаючи штангу близько до тіла та корпус напруженим.", MuscleGroup.FullBody, Equipment.Barbell),
            new("Тяга штанги в нахилі", "У нахилі тягніть штангу до нижньої частини грудей або живота, не округлюючи спину.", MuscleGroup.Back, Equipment.Barbell),
            new("Згинання рук з гантелями", "Згинайте руки в ліктях, піднімаючи гантелі до плечей без розгойдування корпуса.", MuscleGroup.Biceps, Equipment.Dumbbells),
            new("Розгинання рук на блоці", "Розгинайте руки вниз на блоці, фіксуючи лікті біля корпуса та контролюючи повернення.", MuscleGroup.Triceps, Equipment.Cable),
            new("Французький жим", "Згинайте та розгинайте руки в ліктях з вагою над головою або лежачи, контролюючи плечі та амплітуду.", MuscleGroup.Triceps, Equipment.Dumbbells),
            new("Розведення гантелей в сторони", "Піднімайте гантелі в сторони до рівня плечей, зберігаючи легкий згин у ліктях.", MuscleGroup.Shoulders, Equipment.Dumbbells),
            new("Жим ногами", "Виштовхуйте платформу ногами, не блокуючи коліна повністю та не відриваючи таз від сидіння.", MuscleGroup.Legs, Equipment.Machine),
            new("Згинання ніг у тренажері", "Згинайте ноги в колінах у тренажері, контролюючи рух і не підкидаючи вагу інерцією.", MuscleGroup.Legs, Equipment.Machine),
            new("Розгинання ніг у тренажері", "Розгинайте ноги в колінах, коротко фіксуючи верхню точку та повільно повертаючись назад.", MuscleGroup.Legs, Equipment.Machine),
            new("Кроки на платформу", "Піднімайтесь на платформу однією ногою, повністю контролюючи коліно та положення таза.", MuscleGroup.Legs, Equipment.Bench),
            new("Берпі", "Виконайте присідання, перехід в упор лежачи, повернення до ніг і стрибок, зберігаючи контроль техніки.", MuscleGroup.FullBody, Equipment.Bodyweight)
        ];

        public static async Task SeedAsync(
            FitLeadDbContext dbContext,
            CancellationToken cancellationToken = default)
        {
            var existingNames = await dbContext.Exercises
                .Where(x => x.Source == ExerciseSource.Platform)
                .Select(x => x.Name)
                .ToListAsync(cancellationToken);

            var existingNameSet = existingNames.ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var item in Exercises)
            {
                if (existingNameSet.Contains(item.Name))
                    continue;

                var exerciseResult = Exercise.CreatePlatformExercise(
                    item.Name,
                    item.Description,
                    muscleGroup: item.MuscleGroup,
                    equipment: item.Equipment);

                if (exerciseResult.IsFailure)
                    throw new InvalidOperationException(exerciseResult.Error.Message);

                dbContext.Exercises.Add(exerciseResult.Value);
                existingNameSet.Add(item.Name);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public sealed record PlatformExerciseSeedItem(
        string Name,
        string Description,
        MuscleGroup? MuscleGroup,
        Equipment? Equipment);
}
