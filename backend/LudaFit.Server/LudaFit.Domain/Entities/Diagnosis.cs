using System.Net;
using LudaFit.Domain.Entities.Common;
using LudaFit.SharedKernel.Models;

namespace LudaFit.Domain.Entities;

public sealed class Diagnosis : BaseEntity
{
    public string Name { get; private set; } = null!;

    public Booking Booking { get; init; } = null!;
    public int BookingId { get; init; }

    private readonly List<Medicine> _medicines = [];
    public IReadOnlyCollection<Medicine> Medicines => _medicines.AsReadOnly();

    private Diagnosis() { }

    private Diagnosis(string name, Booking booking)
    {
        Name = name;
        Booking = booking;
        BookingId = booking.Id;
    }

    public static Result<Diagnosis> Create(string name, Booking booking)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new ErrorDetails("Назва діагнозу не може бути пустою");
        }
        
        return new Diagnosis(name, booking);
    }
    
    public static Result<Diagnosis> Create(string name, Booking booking, IReadOnlyCollection<string> medicinesNames)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new ErrorDetails("Назва діагнозу не може бути пустою");
        }

        Diagnosis diagnosis = new(name, booking);
        
        Result addMedicinesResult = diagnosis.AddMedicines(medicinesNames);

        if (addMedicinesResult.IsFailure)
        {
            return Result<Diagnosis>.Failure(addMedicinesResult);
        }
        
        return diagnosis;
    }

    public Result ChangeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new ErrorDetails("Назва діагнозу не може бути пустою");
        }

        Name = name;
        return Result.Success();
    }

    public Result AddMedicine(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return new ErrorDetails("Назва ліків не може бути пустою");
        }
        
        string trimmedName = name.Trim();
        bool alreadyExists = _medicines.Any(medicine => medicine.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
        {
            return new ErrorDetails(
                $"Ліки з {nameof(name)}: {name} вже існують",
                HttpStatusCode.Conflict
            );
        }
        
        Result<Medicine> createMedicineResult = Medicine.Create(trimmedName, this);

        if (createMedicineResult.IsFailure)
        {
            return createMedicineResult;
        }
        
        _medicines.Add(createMedicineResult.Value!);
        return Result.Success();
    }

    public Result AddMedicines(IReadOnlyCollection<string> names)
    {
        string[] uniqueNames = names
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        
        List<Medicine> newMedicines = new(uniqueNames.Length);
        
        foreach (string name in uniqueNames)
        {
            Result<Medicine> createMedicineResult = Medicine.Create(name, this);

            if (createMedicineResult.IsFailure)
            {
                return createMedicineResult;
            }
            
            bool alreadyExists = _medicines.Any(medicine => medicine.Name.Trim().Equals(name, StringComparison.OrdinalIgnoreCase))
                || newMedicines.Any(medicine => medicine.Name.Trim().Equals(name, StringComparison.OrdinalIgnoreCase));
            
            if (alreadyExists)
            {
                return new ErrorDetails(
                    $"Ліки з {nameof(name)}: {name} вже існують",
                    HttpStatusCode.Conflict
                );
            }
            
            newMedicines.Add(createMedicineResult.Value!);
        }
        
        _medicines.AddRange(newMedicines);
        return Result.Success();
    }

    public Result DeleteMedicine(int id)
    {
        Medicine? medicine = _medicines.FirstOrDefault(medicine => medicine.Id == id);

        if (medicine is null)
        {
            return new ErrorDetails(
                $"Ліків з {nameof(id)}: {id} не знайдено",
                HttpStatusCode.NotFound
            );
        }

        _medicines.Remove(medicine);
        return Result.Success();
    }

    public Result DeleteMedicine(string name)
    {
        Medicine? medicine = _medicines.FirstOrDefault(medicine => medicine.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));

        if (medicine is null)
        {
            return new ErrorDetails(
                $"Ліків з {nameof(name)}: {name} не знайдено",
                HttpStatusCode.NotFound
            );
        }

        _medicines.Remove(medicine);
        return Result.Success();
    }

    public void DeleteAllMedicines()
        => _medicines.Clear();

    public Result ChangeMedicine(int id, string name)
    {
        Medicine? medicine = _medicines.FirstOrDefault(medicine => medicine.Id == id);

        if (medicine is null)
        {
            return new ErrorDetails(
                $"Ліків з {nameof(id)}: {id} не знайдено",
                HttpStatusCode.NotFound
            );
        }

        Result changeNameResult = medicine.ChangeName(name);

        if (changeNameResult.IsFailure)
        {
            return changeNameResult;
        }

        return Result.Success();
    }
}
