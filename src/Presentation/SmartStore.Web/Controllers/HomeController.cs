using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web.Mvc;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Email;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages;
using SmartStore.Services.Search;
using SmartStore.Services.Seo;
using SmartStore.Services.Topics;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Framework.Filters;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Services.Tasks;
using SmartStore.Web.Models.Home;
using SmartStore.Services.Hyip;
using SmartStore.Core.Domain.Hyip;
using System.Linq;
using SmartStore.Core;
using System.Web;

namespace SmartStore.Web.Controllers
{
    public partial class HomeController : PublicControllerBase
	{
		private readonly ICommonServices _services;
		private readonly Lazy<ICategoryService> _categoryService;
		private readonly Lazy<IProductService> _productService;
		private readonly Lazy<IManufacturerService> _manufacturerService;
		private readonly Lazy<ICatalogSearchService> _catalogSearchService;
		private readonly Lazy<CatalogHelper> _catalogHelper;
		private readonly Lazy<ITopicService> _topicService;
		private readonly Lazy<IXmlSitemapGenerator> _sitemapGenerator;
		private readonly Lazy<CaptchaSettings> _captchaSettings;
		private readonly Lazy<CommonSettings> _commonSettings;
		private readonly Lazy<SeoSettings> _seoSettings;
		private readonly Lazy<CustomerSettings> _customerSettings;
		private readonly Lazy<PrivacySettings> _privacySettings;
		private readonly IScheduleTaskService _scheduleTaskService;
		private readonly ITransactionService _transcationService;
		private readonly IWorkContext _workContext;
		private readonly ICustomerService _customerService;
		private readonly AdminAreaSettings _adminAreaSettings;
		private readonly IPlanService _planService;
		public HomeController(
			ICommonServices services,
			Lazy<ICategoryService> categoryService,
			Lazy<IProductService> productService,
			Lazy<IManufacturerService> manufacturerService,
			Lazy<ICatalogSearchService> catalogSearchService,
			Lazy<CatalogHelper> catalogHelper,
			Lazy<ITopicService> topicService,
			Lazy<IXmlSitemapGenerator> sitemapGenerator,
			Lazy<CaptchaSettings> captchaSettings,
			Lazy<CommonSettings> commonSettings,
			Lazy<SeoSettings> seoSettings,
			Lazy<CustomerSettings> customerSettings,
			Lazy<PrivacySettings> privacySettings,
			IScheduleTaskService scheduleTaskService,
			ITransactionService transactionService,
			IWorkContext workContext,
			ICustomerService customerService, AdminAreaSettings adminAreaSettings,
			IPlanService planService)
        {
			this._services = services;
			this._categoryService = categoryService;
			this._productService = productService;
			this._manufacturerService = manufacturerService;
			this._catalogSearchService = catalogSearchService;
			this._catalogHelper = catalogHelper;
			this._topicService = topicService;
			this._sitemapGenerator = sitemapGenerator;
			this._captchaSettings = captchaSettings;
			this._commonSettings = commonSettings;
			this._seoSettings = seoSettings;
            this._customerSettings = customerSettings;
			this._privacySettings = privacySettings;
			this._scheduleTaskService = scheduleTaskService;
			this._transcationService = transactionService;
			this._workContext = workContext;
			this._customerService = customerService;
			this._adminAreaSettings = adminAreaSettings;
			this._planService = planService;
		}
		
        [RequireHttpsByConfig(SslRequirement.No)]
        public ActionResult Index()
        {
			//string domainName = HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);
			//if(domainName == "http://bahtbucket.net"|| domainName == "https://bahtbucket.net"
			//	|| domainName == "https://www.bahtbucket.net"
			//	|| domainName == "http://www.bahtbucket.net" || domainName == "http://localhost:53161" || domainName == "http://localhost" || domainName == "http://localhost:81")
			//{
				
			//}
			//else
			//{
			//	return Content("Invalid License, call us on +91-9737443888");
			//}
			//var task = new ScheduleTask
			//{
			//	CronExpression = "0 */24 * * *",
			//	Type = "SmartStore.Services.Hyip.CheckWithdrawalStatusTask, SmartStore.Services",//typeof("SmartStore.Services.Hyip.SendROITask,SmartStore.Services").AssemblyQualifiedNameWithoutVersion(),
			//	Enabled = false,
			//	StopOnError = false,
			//	IsHidden = false
			//};

			//task.Name = string.Concat("Check Withdrawal Status", " Task");
			//_scheduleTaskService.InsertTask(task);
			if(Request["r"] != null)
			{
				HttpCookie cookie = new HttpCookie("mthreferral", Request["r"].ToSafe());
				HttpContext.Response.Cookies.Remove("mthreferral");
				HttpContext.Response.SetCookie(cookie);
			}
			var reff = Request.Cookies["mthreferral"];
			//var customers = _customerService.GetOnlineCustomers(DateTime.UtcNow.AddMinutes(-_customerSettings.Value.OnlineCustomerMinutes),
			//	null, 0, _adminAreaSettings.GridPageSize);

			var customers = _customerService.GetAllCustomers(null,null,null,null,null,null,null,0,0,null,null,null,false,null,0,int.MaxValue,false);
			HomeModel homemodel = new HomeModel();
			homemodel.LaunchDate = DateTime.Parse("08/01/2019").ToShortDateString(); //DateTime.Today.AddDays(-95).ToShortDateString();
			TimeSpan dt = DateTime.Now - DateTime.Parse(homemodel.LaunchDate);
			homemodel.RunningDays = dt.Days.ToString();
			homemodel.OnlineVistors = customers.Count().ToString();
			homemodel.TotalCustomer = customers.Where(x => x.Email != null).Count().ToString();
			int[] StatusIds = { (int)Status.Completed };
			int[] TranscationTypeIds = { (int)TransactionType.Purchase };
			var totalfundinglist = _transcationService.GetAllTransactions(0, 0, null, null, StatusIds, TranscationTypeIds, 0, int.MaxValue).OrderByDescending(x=>x.TransactionDate).Take(10);
			homemodel.TotalSiteDeposit = _transcationService.GetAllTransactions(0, 0, null, null, StatusIds, TranscationTypeIds, 0, int.MaxValue).Select(x => x.Amount).Sum().ToString();
			int[] WithTranscationTypeIds = { (int)TransactionType.Withdrawal };
			var totalWithdrawallist = _transcationService.GetAllTransactions(0, 0, null, null, StatusIds, WithTranscationTypeIds, 0, int.MaxValue).OrderByDescending(x => x.TransactionDate).Take(10);
			homemodel.TotalSiteWithdrawal = _transcationService.GetAllTransactions(0, 0, null, null, StatusIds, WithTranscationTypeIds, 0, int.MaxValue).Select(x => x.Amount).Sum().ToString();

			var withdrawals = totalWithdrawallist.OrderByDescending(x => x.TransactionDate).ToList();
			foreach(var d in withdrawals)
			{
				WithdrawalList w = new WithdrawalList();
				w.Email = "AP-"+d.Customer.Id;
				w.AmountRaw = d.Amount.ToString() + _workContext.WorkingCurrency.CurrencyCode;
				w.TransDate = d.TransactionDate.ToShortDateString();
				w.PaymentMethodIcon = d.ProcessorId + ".svg";
				homemodel.Last10Withdrawal.Add(w);
			}

			var deposits = totalfundinglist.OrderByDescending(x => x.TransactionDate).ToList();
			foreach (var d in deposits)
			{
				DepositList w = new DepositList();
				w.Email = "AP-" + d.Customer.Id;
				w.AmountRaw = d.Amount.ToString() + _workContext.WorkingCurrency.CurrencyCode;
				w.TransDate = d.TransactionDate.ToShortDateString();
				w.PaymentMethodIcon = d.ProcessorId + ".svg";
				homemodel.Last10Deposit.Add(w);
			}
			homemodel.plans = _planService.GetAllPackage().ToList();
			ViewBag.SiteName = "AlienProfit";
			return View(homemodel);
        }

		[HttpPost]
		public ActionResult CalculateReturn(float amountinvested,int planid)
		{
			var plan = _planService.GetPlanById(planid);
			if(plan != null)
			{
				var profit = 0.00;
				if(amountinvested >= plan.MinimumInvestment && amountinvested <= plan.MaximumInvestment)
				{
					if (plan.IsPrincipalBack)
						profit = (((plan.ROIPercentage * amountinvested) / 100) * plan.NoOfPayouts) + amountinvested;
					else
						profit = (((plan.ROIPercentage * amountinvested) / 100) * plan.NoOfPayouts);

					return new JsonResult
					{
						Data = new { success = true, totalprofit = profit }
					};
				}
				else
				{
					return new JsonResult
					{
						Data = new { success = false, totalprofit = 0, Message = "Please enter valid investment" }
					};
				}
			}
			return new JsonResult
			{
				Data = new { success = false, totalprofit = 0 }
			};
		}
		public ActionResult Plan()
		{
			var plans = _planService.GetAllPlans().ToList();
			return View(plans);
		}
		public ActionResult StoreClosed()
		{
			return View();
		}

		[RequireHttpsByConfig(SslRequirement.No)]
		[GdprConsent]
		public ActionResult ContactUs()
		{
            var topic = _topicService.Value.GetTopicBySystemName("ContactUs");

            var model = new ContactUsModel
			{
				Email = _services.WorkContext.CurrentCustomer.Email,
				FullName = _services.WorkContext.CurrentCustomer.GetFullName(),
				FullNameRequired = _privacySettings.Value.FullNameOnContactUsRequired,
				DisplayCaptcha = _captchaSettings.Value.Enabled && _captchaSettings.Value.ShowOnContactUsPage,
                MetaKeywords = topic?.GetLocalized(x => x.MetaKeywords),
                MetaDescription = topic?.GetLocalized(x => x.MetaDescription),
                MetaTitle = topic?.GetLocalized(x => x.MetaTitle),
            };

			return View(model);
		}

		[HttpPost, ActionName("ContactUs")]
		[ValidateCaptcha, ValidateHoneypot]
		[GdprConsent]
		public ActionResult ContactUsSend(ContactUsModel model, bool captchaValid)
		{
			// Validate CAPTCHA
			if (_captchaSettings.Value.Enabled && _captchaSettings.Value.ShowOnContactUsPage && !captchaValid)
			{
				ModelState.AddModelError("", T("Common.WrongCaptcha"));
			}

			if (ModelState.IsValid)
			{
				var customer = _services.WorkContext.CurrentCustomer;
				var email = model.Email.Trim();
				var fullName = model.FullName;
				var subject = T("ContactUs.EmailSubject", _services.StoreContext.CurrentStore.Name);
				var body = Core.Html.HtmlUtils.FormatText(model.Enquiry, false, true, false, false, false, false);

				// Required for some SMTP servers
				EmailAddress sender = null;
				if (!_commonSettings.Value.UseSystemEmailForContactUsForm)
				{
					sender = new EmailAddress(email, fullName);
				}

				// email
				var msg = Services.MessageFactory.SendContactUsMessage(customer, email, fullName, subject, body, sender);

				if (msg?.Email?.Id != null)
				{
					model.SuccessfullySent = true;
					model.Result = T("ContactUs.YourEnquiryHasBeenSent");
					_services.CustomerActivity.InsertActivity("PublicStore.ContactUs", T("ActivityLog.PublicStore.ContactUs"));
				}
				else
				{
					ModelState.AddModelError("", T("Common.Error.SendMail"));
					model.Result = T("Common.Error.SendMail");
				}

				return View(model);
			}

			model.DisplayCaptcha = _captchaSettings.Value.Enabled && _captchaSettings.Value.ShowOnContactUsPage;
			return View(model);
		}

		[RequireHttpsByConfigAttribute(SslRequirement.No)]
		public ActionResult SitemapSeo(int? index = null)
		{
			if (!_seoSettings.Value.XmlSitemapEnabled)
				return HttpNotFound();
			
			string content = _sitemapGenerator.Value.GetSitemap(index);

			if (content == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Sitemap index is out of range.");
			}

			return Content(content, "text/xml", Encoding.UTF8);
		}

		[RequireHttpsByConfigAttribute(SslRequirement.No)]
		public ActionResult Sitemap()
		{
            return RedirectPermanent(_services.StoreContext.CurrentStore.Url);
		}

		
    }
}
