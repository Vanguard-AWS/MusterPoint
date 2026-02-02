using MusterPoint.Client.Models;

namespace MusterPoint.Client.Services
{
    /// <summary>
  /// Service for validating 40K army lists according to game rules
    /// </summary>
  public class ListValidationService
    {
      /// <summary>
        /// Represents a validation error or warning
 /// </summary>
        public class ValidationMessage
        {
         public string Message { get; set; } = string.Empty;
public ValidationSeverity Severity { get; set; }
 }

        public enum ValidationSeverity
     {
          Error,
         Warning,
            Info
        }

        /// <summary>
     /// Validates a list against all 40K army composition rules
     /// </summary>
     public List<ValidationMessage> ValidateList(UserList list, List<Unit>? allUnits, int pointsLimit = 2000)
        {
      var messages = new List<ValidationMessage>();

  if (list?.Entries == null || allUnits == null)
    return messages;

    // Rule 1: At least one CHARACTER
            ValidateAtLeastOneCharacter(list, allUnits, messages);

  // Rule 2: Only one of each named EPIC HERO
      ValidateEpicHeroes(list, allUnits, messages);

            // Rule 3: Up to three units of each datasheet (with exceptions for BATTLELINE/DEDICATED TRANSPORT)
 ValidateUnitLimits(list, allUnits, messages);

   // Rule 4: Each CHARACTER can only have one Enhancement, max three total, all different
 ValidateEnhancements(list, allUnits, messages);

  // Rule 5: Exactly one Warlord
     ValidateWarlord(list, messages);

    // Rule 6: Points limit
    ValidatePointsLimit(list, messages, pointsLimit);

            return messages;
        }

        private void ValidateAtLeastOneCharacter(UserList list, List<Unit> allUnits, List<ValidationMessage> messages)
        {
            var hasCharacter = list.Entries.Any(entry =>
            {
      var unit = allUnits.FirstOrDefault(u => u.Id == entry.UnitId);
    return unit != null && unit.Category == "Characters";
        });

       if (!hasCharacter)
  {
              messages.Add(new ValidationMessage
     {
        Message = "You must include at least one CHARACTER unit in your list.",
                    Severity = ValidationSeverity.Error
    });
       }
      }

        private void ValidateEpicHeroes(UserList list, List<Unit> allUnits, List<ValidationMessage> messages)
        {
   var epicHeroes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "EPIC HERO" };

     var heroEntries = list.Entries
 .Where(entry =>
        {
   var unit = allUnits.FirstOrDefault(u => u.Id == entry.UnitId);
     return unit != null && unit.Keywords.Any(k => epicHeroes.Contains(k));
       })
       .GroupBy(e => e.UnitId)
          .Where(g => g.Count() > 1)
          .ToList();

        foreach (var group in heroEntries)
   {
 var unit = allUnits.FirstOrDefault(u => u.Id == group.Key);
    if (unit != null)
    {
    messages.Add(new ValidationMessage
         {
             Message = $"You can only include one {unit.Name} (EPIC HERO) in your list, but you have {group.Count()}.",
    Severity = ValidationSeverity.Error
   });
                }
    }
        }

        private void ValidateUnitLimits(UserList list, List<Unit> allUnits, List<ValidationMessage> messages)
        {
      var unitGroups = list.Entries
           .GroupBy(e => e.UnitId)
 .ToList();

 foreach (var group in unitGroups)
      {
                var unit = allUnits.FirstOrDefault(u => u.Id == group.Key);
           if (unit == null)
      continue;

         var count = group.Count();
          var isBattleline = unit.Keywords.Contains("BATTLELINE", StringComparer.OrdinalIgnoreCase);
       var isDedicatedTransport = unit.Keywords.Contains("DEDICATED TRANSPORT", StringComparer.OrdinalIgnoreCase);
           var maxAllowed = (isBattleline || isDedicatedTransport) ? 6 : 3;

 if (count > maxAllowed)
     {
    var limitText = (isBattleline || isDedicatedTransport)
          ? "up to six units"
       : "up to three units";
        messages.Add(new ValidationMessage
  {
       Message = $"You can only include {limitText} of {unit.Name}, but you have {count}.",
    Severity = ValidationSeverity.Error
          });
    }
       }
        }

        private void ValidateEnhancements(UserList list, List<Unit> allUnits, List<ValidationMessage> messages)
        {
            var enhancedCharacters = list.Entries
    .Where(e => !string.IsNullOrEmpty(e.Enhancement))
    .ToList();

    // Rule 4a: Each CHARACTER can only have one Enhancement
      var charactersWithMultipleEnhancements = enhancedCharacters
       .GroupBy(e => e.UnitId)
    .Where(g => g.Count() > 1)
         .ToList();

            foreach (var group in charactersWithMultipleEnhancements)
         {
   var unit = allUnits.FirstOrDefault(u => u.Id == group.Key);
 if (unit != null)
 {
  messages.Add(new ValidationMessage
  {
           Message = $"Each CHARACTER can only have one Enhancement, but {unit.Name} has {group.Count()}.",
     Severity = ValidationSeverity.Error
          });
     }
   }

   // Rule 4b: Max three Enhancements total
            if (enhancedCharacters.Count > 3)
        {
       messages.Add(new ValidationMessage
    {
    Message = $"You can include a maximum of three Enhancements in total, but you have {enhancedCharacters.Count()}.",
  Severity = ValidationSeverity.Error
        });
   }

   // Rule 4c: All Enhancements must be different
     var enhancementIds = enhancedCharacters
    .Where(e => !string.IsNullOrEmpty(e.Enhancement))
      .Select(e => e.Enhancement)
     .ToList();

    var duplicateEnhancements = enhancementIds
.GroupBy(e => e)
    .Where(g => g.Count() > 1)
        .ToList();

  if (duplicateEnhancements.Any())
 {
       messages.Add(new ValidationMessage
     {
 Message = "All Enhancements must be different. You have duplicate Enhancements applied.",
 Severity = ValidationSeverity.Error
     });
         }
        }

      private void ValidateWarlord(UserList list, List<ValidationMessage> messages)
        {
            var warlords = list.Entries.Where(e => e.IsWarlord).ToList();

         if (warlords.Count == 0)
     {
       messages.Add(new ValidationMessage
     {
      Message = "You must select one CHARACTER to be your Warlord.",
    Severity = ValidationSeverity.Error
                });
         }
       else if (warlords.Count > 1)
            {
     messages.Add(new ValidationMessage
       {
       Message = $"You can only have one Warlord, but you have {warlords.Count} selected.",
   Severity = ValidationSeverity.Error
      });
        }
        }

    private void ValidatePointsLimit(UserList list, List<ValidationMessage> messages, int pointsLimit)
    {
        var totalPoints = list.CalculateTotalPoints();
        
   if (totalPoints > pointsLimit)
        {
      var overage = totalPoints - pointsLimit;
       messages.Add(new ValidationMessage
  {
   Message = $"Your list exceeds the {pointsLimit} point limit by {overage} points. ({totalPoints} / {pointsLimit} pts)",
                Severity = ValidationSeverity.Warning
          });
        }
    }

    /// <summary>
    /// Determines if a list is valid (has no errors)
    /// </summary>
 public bool IsListValid(List<ValidationMessage> messages)
    {
        return !messages.Any(m => m.Severity == ValidationSeverity.Error);
    }

    /// <summary>
    /// Gets all error messages
    /// </summary>
    public List<ValidationMessage> GetErrors(List<ValidationMessage> messages)
    {
 return messages.Where(m => m.Severity == ValidationSeverity.Error).ToList();
    }

    /// <summary>
    /// Gets all warning messages
    /// </summary>
    public List<ValidationMessage> GetWarnings(List<ValidationMessage> messages)
    {
        return messages.Where(m => m.Severity == ValidationSeverity.Warning).ToList();
    }
}
}
