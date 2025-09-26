using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Domain.Entities;
using WhiteLagoon.ViewModels;
using WhiteLagoon.Application.Utility.Helpers;
using WhiteLagoon.Application.Utility.Constants;
using Syncfusion.Presentation;
using WhiteLagoon.Application.Services.Interfaces;

namespace WhiteLagoon.Controllers;

public class HomeController(IVillaService villaService, IWebHostEnvironment webHostEnvironment) : Controller
{
	public async Task<IActionResult> Index()
	{
		return View(await GetHomeViewModelAsync());
	}

	[HttpPost]
	public async Task<IActionResult> GetVillasByDate(int nights, DateOnly checkInDate)
	{
		var homeViewModel = new HomeViewModel
		{
			VillaList = await villaService.GetVillasAvailabilityByDate(nights, checkInDate),
			Nights = nights,
			CheckInDate = checkInDate
		};

		return PartialView("_VillaList", homeViewModel);
	}

	[HttpPost]
	public async Task<IActionResult> GeneratePPTExport(int id)
	{
		var villa = await villaService.GetVillaByIdAsync(id);
		if (villa == null)
		{
			TempData["error"] = "Villa not found.";
			return RedirectToAction(nameof(Error));
		}

		var basePath = webHostEnvironment.WebRootPath;
		var filePath = Path.Combine(basePath, "exports", "ExportVillaDetails.pptx");

		using IPresentation presentation = Presentation.Open(filePath);

		ISlide slide = presentation.Slides[0];

		IShape? shape = slide.Shapes.FirstOrDefault(s => s.ShapeName == "txtVillaName") as IShape;
		if (shape is not null)
		{
			shape.TextBody.Text = villa.Name;
		}

		shape = slide.Shapes.FirstOrDefault(s => s.ShapeName == "txtVillaDescription") as IShape;
		if (shape is not null)
		{
			shape.TextBody.Text = villa.Description;
		}

		shape = slide.Shapes.FirstOrDefault(s => s.ShapeName == "txtVillaOccupancy") as IShape;
		if (shape is not null)
		{
			shape.TextBody.Text = string.Format("Max occupancy: {0} adults", villa.Occupancy);
		}

		shape = slide.Shapes.FirstOrDefault(s => s.ShapeName == "txtVillaSize") as IShape;
		if (shape is not null)
		{
			shape.TextBody.Text = string.Format("Villa size: {0} sq ft", villa.Sqft);
		}

		shape = slide.Shapes.FirstOrDefault(s => s.ShapeName == "txtPricePerNight") as IShape;
		if (shape is not null)
		{
			shape.TextBody.Text = string.Format("USD {0} / night", villa.Price.ToString("c"));
		}

		shape = slide.Shapes.FirstOrDefault(s => s.ShapeName == "txtVillaAmenitiesHeading") as IShape;
		if (shape is not null)
		{
			var listItems = villa.VillaAmenities.Select(x => x.Name).ToList();
			shape.TextBody.Text = string.Empty;

			foreach (var item in listItems)
			{
				IParagraph paragraph = shape.TextBody.AddParagraph();
				ITextPart textPart = paragraph.AddTextPart(item);
				paragraph.ListFormat.Type = ListType.Bulleted;
				paragraph.ListFormat.BulletCharacter = '\u2022';
				textPart.Font.Color = ColorObject.FromArgb(144, 148, 152);
			}
		}

		shape = slide.Shapes.FirstOrDefault(s => s.ShapeName == "imgVilla") as IShape;
		if (shape is not null)
		{
			byte[] imageData = [];
			string imageUrl;

			try
			{
				imageUrl = string.Format("{0}{1}", basePath, villa.ImageUrl);
				imageData = System.IO.File.ReadAllBytes(imageUrl);
			}
			catch (Exception)
			{
				imageUrl = string.Format("{0}{1}", basePath, "/images/placeholder.png");
				imageData = System.IO.File.ReadAllBytes(imageUrl);
			}

			slide.Shapes.Remove(shape);

			using MemoryStream imageStream = new(imageData);
			IPicture newPicture = slide.Pictures.AddPicture(imageStream, 60, 120, 300, 200);
		}

		MemoryStream memoryStream = new();
		presentation.Save(memoryStream);
		memoryStream.Position = 0;

		return File(memoryStream, "application/pptx", $"Villa.pptx");
	}

	public IActionResult Privacy()
	{
		return View();
	}

	public IActionResult Error()
	{
		return View();
	}

	private async Task<HomeViewModel> GetHomeViewModelAsync()
	{
		var homeViewModel = new HomeViewModel
		{
			VillaList = await villaService.GetAllVillasAsync(),
			Nights = 1,
			CheckInDate = DateOnly.FromDateTime(DateTime.Now)
		};

		return homeViewModel;
	}
}
