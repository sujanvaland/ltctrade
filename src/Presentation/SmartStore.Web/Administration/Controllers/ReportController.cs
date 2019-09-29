﻿using SmartStore.Admin.Models.Investment;
using SmartStore.Admin.Models.PaymentMethods;
using SmartStore.Core;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Hyip;
using SmartStore.Core.Domain.Localization;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Hyip;
using SmartStore.Services.Localization;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
	public class ReportController : AdminControllerBase
	{
		private readonly IPlanService _planService;
		private readonly ICustomerPlanService _customerPlanService;
		private readonly ITransactionService _transactionService;
		private readonly IDateTimeHelper _dateTimeHelper;
		private readonly IWorkContext _workContext;
		private readonly AdminAreaSettings _adminAreaSettings;
		private readonly ILocalizationService _localizationService;
		private readonly IPriceFormatter _priceFormatter;
		private readonly ICurrencyService _currencyService;
		private readonly IStoreService _storeService;
		private readonly ICommonServices _services;
		private readonly ICustomerService _customerService;
		private readonly LocalizationSettings _localizationSettings;
		public ReportController(IPlanService planService,
			ICustomerPlanService customerPlanService,
			IDateTimeHelper dateTimeHelper,
			IWorkContext workContext,
			ITransactionService transactionService,
			AdminAreaSettings adminAreaSettings,
			ILocalizationService localizationService,
			IPriceFormatter priceFormatter,
			ICurrencyService currencyService,
			IStoreService storeService,
			ICommonServices services,
			ICustomerService customerService,
			LocalizationSettings localizationSettings)
		{
			_planService = planService;
			_customerPlanService = customerPlanService;
			_dateTimeHelper = dateTimeHelper;
			_workContext = workContext;
			_transactionService = transactionService;
			_adminAreaSettings = adminAreaSettings;
			_localizationService = localizationService;
			_priceFormatter = priceFormatter;
			_currencyService = currencyService;
			_storeService = storeService;
			_services = services;
			_customerService = customerService;
			_localizationSettings = localizationSettings;
		}
		// GET: Report
		public ActionResult PurchaseReport()
		{
			TransactionModel model = new TransactionModel();
			model.AvailableStatus = Status.All.ToSelectList(false).ToList();
			if (_workContext.CurrentCustomer.IsInCustomerRole("Administrators"))
			{
				model.IsVisible = true;
			}
			model.GridPageSize = _adminAreaSettings.GridPageSize;

			return View(model);
		}

		public ActionResult WithdrawalReport()
		{
			TransactionModel model = new TransactionModel();
			model.AvailableStatus = Status.All.ToSelectList(false).ToList();
			if (_workContext.CurrentCustomer.IsInCustomerRole("Administrators"))
			{
				model.IsVisible = true;
			}
			model.GridPageSize = _adminAreaSettings.GridPageSize;

			return View(model);
		}

		public ActionResult CommissionReport()
		{
			TransactionModel model = new TransactionModel();
			model.AvailableStatus = Status.All.ToSelectList(false).ToList();
			if (_workContext.CurrentCustomer.IsInCustomerRole("Administrators"))
			{
				model.IsVisible = true;
			}
			model.GridPageSize = _adminAreaSettings.GridPageSize;

			return View(model);
		}

		public ActionResult DirectBonusReport()
		{
			TransactionModel model = new TransactionModel();
			model.AvailableStatus = Status.All.ToSelectList(false).ToList();
			if (_workContext.CurrentCustomer.IsInCustomerRole("Administrators"))
			{
				model.IsVisible = true;
			}
			model.GridPageSize = _adminAreaSettings.GridPageSize;

			return View(model);
		}

		public ActionResult UnilevelBonusReport()
		{
			TransactionModel model = new TransactionModel();
			model.AvailableStatus = Status.All.ToSelectList(false).ToList();
			if (_workContext.CurrentCustomer.IsInCustomerRole("Administrators"))
			{
				model.IsVisible = true;
			}
			model.GridPageSize = _adminAreaSettings.GridPageSize;

			return View(model);
		}

		public ActionResult CyclerBonusReport()
		{
			TransactionModel model = new TransactionModel();
			model.AvailableStatus = Status.All.ToSelectList(false).ToList();
			if (_workContext.CurrentCustomer.IsInCustomerRole("Administrators"))
			{
				model.IsVisible = true;
			}
			model.GridPageSize = _adminAreaSettings.GridPageSize;

			return View(model);
		}

		public ActionResult PoolBonusReport()
		{
			TransactionModel model = new TransactionModel();
			model.AvailableStatus = Status.All.ToSelectList(false).ToList();
			if (_workContext.CurrentCustomer.IsInCustomerRole("Administrators"))
			{
				model.IsVisible = true;
			}
			model.GridPageSize = _adminAreaSettings.GridPageSize;

			return View(model);
		}

		[GridAction(EnableCustomBinding = true)]
		public ActionResult ListFunding(GridCommand command, TransactionModel model)
		{
			var gridModel = new GridModel<TransactionModel>();
			DateTime? startDateValue = (model.StartDate == null) ? null : model.StartDate;
			DateTime? endDateValue = (model.EndDate == null) ? null : model.EndDate;

			int[] StatusIds = model.StatusIds.ToIntArray();
			int[] TranscationTypeIds = { (int)TransactionType.Funding };
			var customerid = _workContext.CurrentCustomer.Id;
			bool Is_Visible = true;
			if (_workContext.CurrentCustomer.IsInCustomerRole("Administrators"))
			{
				customerid = 0;
				Is_Visible = false;
			}

			var transcation = _transactionService.GetAllTransactions(0, customerid, startDateValue,endDateValue,
							  StatusIds, TranscationTypeIds,command.Page - 1, command.PageSize);

			var currency = _workContext.WorkingCurrency.CurrencyCode;

			gridModel.Data = transcation.Select(x =>
			{
				var transModel = new TransactionModel
				{
					Id = x.Id,
					FinalAmountRaw = x.Amount +" " + currency,
					FinalAmount = x.Amount,
					TransactionDate = x.TransactionDate,
					IsVisible = Is_Visible,
					StatusId = x.StatusId,
					TransStatusString = x.Status.GetLocalizedEnum(_localizationService,_workContext),
					TranscationTypeString = x.TranscationType.GetLocalizedEnum(_localizationService,_workContext),
					ProcessorId = x.ProcessorId,
					WithdrawalAccount = x.WithdrawalAddress,
					CustomerEmail = x.Customer.Email
				};

				transModel.TransactionDateString = transModel.TransactionDate.ToString("g");
				
				return transModel;
			});

			gridModel.Total = transcation.TotalCount;

			return new JsonResult
			{
				Data = gridModel
			};
		}

		[GridAction(EnableCustomBinding = true)]
		public ActionResult ListWithdrawal(GridCommand command, TransactionModel model)
		{
			var gridModel = new GridModel<TransactionModel>();
			DateTime? startDateValue = (model.StartDate == null) ? null : model.StartDate;
			DateTime? endDateValue = (model.EndDate == null) ? null : model.EndDate;

			int[] StatusIds = model.StatusIds.ToIntArray();
			int[] TranscationTypeIds = { (int)TransactionType.Withdrawal };
			var customerid = _workContext.CurrentCustomer.Id;
			bool Is_Visible = false;
			if (_workContext.CurrentCustomer.IsInCustomerRole("Administrators"))
			{
				customerid = 0;
				Is_Visible = true;
			}

			var transcation = _transactionService.GetAllTransactions(0, customerid, startDateValue, endDateValue,
							  StatusIds, TranscationTypeIds, command.Page - 1, command.PageSize);

			var currency = _workContext.WorkingCurrency.CurrencyCode;
			
			gridModel.Data = transcation.Select(x =>
			{
				var transModel = new TransactionModel
				{
					Id = x.Id,
					FinalAmountRaw = x.Amount + " " + currency,
					FinalAmount = x.Amount,
					TransactionDate = x.TransactionDate,
					StatusId = x.StatusId,
					IsVisible = Is_Visible,
					TransStatusString = x.Status.GetLocalizedEnum(_localizationService, _workContext),
					TranscationTypeString = x.TranscationType.GetLocalizedEnum(_localizationService, _workContext),
					ProcessorId = x.ProcessorId,
					WithdrawalAccount = x.WithdrawalAddress,
					CustomerEmail = x.Customer.Email
				};

				transModel.TransactionDateString = transModel.TransactionDate.ToString("g");

				return transModel;
			});

			gridModel.Total = transcation.TotalCount;

			return new JsonResult
			{
				Data = gridModel
			};
		}

		[GridAction(EnableCustomBinding = true)]
		public ActionResult ListUnilevelBonus(GridCommand command, TransactionModel model)
		{
			var gridModel = new GridModel<TransactionModel>();
			DateTime? startDateValue = (model.StartDate == null) ? null : model.StartDate;
			DateTime? endDateValue = (model.EndDate == null) ? null : model.EndDate;

			int[] StatusIds = model.StatusIds.ToIntArray();
			int[] TranscationTypeIds = { (int)TransactionType.UnilevelBonus };
			var customerid = _workContext.CurrentCustomer.Id;
			bool Is_Visible = false;
			if (_workContext.CurrentCustomer.IsInCustomerRole("Administrators"))
			{
				customerid = 0;
				Is_Visible = true;
			}

			var transcation = _transactionService.GetAllTransactions(0, customerid, startDateValue, endDateValue,
							  StatusIds, TranscationTypeIds, command.Page - 1, command.PageSize);

			var currency = _workContext.WorkingCurrency.CurrencyCode;

			gridModel.Data = transcation.Select(x =>
			{
				var transModel = new TransactionModel
				{
					Id = x.Id,
					FinalAmountRaw = x.Amount + " " + currency,
					FinalAmount = x.Amount,
					TransactionDate = x.TransactionDate,
					StatusId = x.StatusId,
					IsVisible = Is_Visible,
					TransStatusString = x.Status.GetLocalizedEnum(_localizationService, _workContext),
					TranscationTypeString = x.TranscationType.GetLocalizedEnum(_localizationService, _workContext),
					ProcessorId = x.ProcessorId,
					CustomerEmail = x.Customer.Email
				};

				transModel.TransactionDateString = transModel.TransactionDate.ToString("g");

				return transModel;
			});

			gridModel.Total = transcation.TotalCount;

			return new JsonResult
			{
				Data = gridModel
			};
		}

		[GridAction(EnableCustomBinding = true)]
		public ActionResult ListDirectBonus(GridCommand command, TransactionModel model)
		{
			var gridModel = new GridModel<TransactionModel>();
			DateTime? startDateValue = (model.StartDate == null) ? null : model.StartDate;
			DateTime? endDateValue = (model.EndDate == null) ? null : model.EndDate;

			int[] StatusIds = model.StatusIds.ToIntArray();
			int[] TranscationTypeIds = { (int)TransactionType.DirectBonus };
			var customerid = _workContext.CurrentCustomer.Id;
			bool Is_Visible = false;
			if (_workContext.CurrentCustomer.IsInCustomerRole("Administrators"))
			{
				customerid = 0;
				Is_Visible = true;
			}

			var transcation = _transactionService.GetAllTransactions(0, customerid, startDateValue, endDateValue,
							  StatusIds, TranscationTypeIds, command.Page - 1, command.PageSize);

			var currency = _workContext.WorkingCurrency.CurrencyCode;

			gridModel.Data = transcation.Select(x =>
			{
				var transModel = new TransactionModel
				{
					Id = x.Id,
					FinalAmountRaw = x.Amount + " " + currency,
					FinalAmount = x.Amount,
					TransactionDate = x.TransactionDate,
					StatusId = x.StatusId,
					IsVisible = Is_Visible,
					TransStatusString = x.Status.GetLocalizedEnum(_localizationService, _workContext),
					TranscationTypeString = x.TranscationType.GetLocalizedEnum(_localizationService, _workContext),
					ProcessorId = x.ProcessorId,
					CustomerEmail = x.Customer.Email
				};

				transModel.TransactionDateString = transModel.TransactionDate.ToString("g");

				return transModel;
			});

			gridModel.Total = transcation.TotalCount;

			return new JsonResult
			{
				Data = gridModel
			};
		}

		[GridAction(EnableCustomBinding = true)]
		public ActionResult ListCyclerBonus(GridCommand command, TransactionModel model)
		{
			var gridModel = new GridModel<TransactionModel>();
			DateTime? startDateValue = (model.StartDate == null) ? null : model.StartDate;
			DateTime? endDateValue = (model.EndDate == null) ? null : model.EndDate;

			int[] StatusIds = model.StatusIds.ToIntArray();
			int[] TranscationTypeIds = { (int)TransactionType.CyclerBonus };
			var customerid = _workContext.CurrentCustomer.Id;
			bool Is_Visible = false;
			if (_workContext.CurrentCustomer.IsInCustomerRole("Administrators"))
			{
				customerid = 0;
				Is_Visible = true;
			}

			var transcation = _transactionService.GetAllTransactions(0, customerid, startDateValue, endDateValue,
							  StatusIds, TranscationTypeIds, command.Page - 1, command.PageSize);

			var currency = _workContext.WorkingCurrency.CurrencyCode;

			gridModel.Data = transcation.Select(x =>
			{
				var transModel = new TransactionModel
				{
					Id = x.Id,
					FinalAmountRaw = x.Amount + " " + currency,
					FinalAmount = x.Amount,
					TransactionDate = x.TransactionDate,
					StatusId = x.StatusId,
					IsVisible = Is_Visible,
					TransStatusString = x.Status.GetLocalizedEnum(_localizationService, _workContext),
					TranscationTypeString = x.TranscationType.GetLocalizedEnum(_localizationService, _workContext),
					ProcessorId = x.ProcessorId,
					CustomerEmail = x.Customer.Email
				};

				transModel.TransactionDateString = transModel.TransactionDate.ToString("g");

				return transModel;
			});

			gridModel.Total = transcation.TotalCount;

			return new JsonResult
			{
				Data = gridModel
			};
		}

		[GridAction(EnableCustomBinding = true)]
		public ActionResult ListPoolBonus(GridCommand command, TransactionModel model)
		{
			var gridModel = new GridModel<TransactionModel>();
			DateTime? startDateValue = (model.StartDate == null) ? null : model.StartDate;
			DateTime? endDateValue = (model.EndDate == null) ? null : model.EndDate;

			int[] StatusIds = model.StatusIds.ToIntArray();
			int[] TranscationTypeIds = { (int)TransactionType.PoolBonus };
			var customerid = _workContext.CurrentCustomer.Id;
			bool Is_Visible = false;
			if (_workContext.CurrentCustomer.IsInCustomerRole("Administrators"))
			{
				customerid = 0;
				Is_Visible = true;
			}

			var transcation = _transactionService.GetAllTransactions(0, customerid, startDateValue, endDateValue,
							  StatusIds, TranscationTypeIds, command.Page - 1, command.PageSize);

			var currency = _workContext.WorkingCurrency.CurrencyCode;

			gridModel.Data = transcation.Select(x =>
			{
				var transModel = new TransactionModel
				{
					Id = x.Id,
					FinalAmountRaw = x.Amount + " " + currency,
					FinalAmount = x.Amount,
					TransactionDate = x.TransactionDate,
					StatusId = x.StatusId,
					IsVisible = Is_Visible,
					TransStatusString = x.Status.GetLocalizedEnum(_localizationService, _workContext),
					TranscationTypeString = x.TranscationType.GetLocalizedEnum(_localizationService, _workContext),
					ProcessorId = x.ProcessorId,
					CustomerEmail = x.Customer.Email
				};

				transModel.TransactionDateString = transModel.TransactionDate.ToString("g");

				return transModel;
			});

			gridModel.Total = transcation.TotalCount;

			return new JsonResult
			{
				Data = gridModel
			};
		}

		[HttpPost]
		public ActionResult CompleteRequest(int Id)
		{
			try
			{
				var transaction = _transactionService.GetTransactionById(Id);
				transaction.StatusId = (int)Status.Completed;
				_transactionService.UpdateTransaction(transaction);
				Services.MessageFactory.SendDepositNotificationMessageToUser(transaction, "", "", _localizationSettings.DefaultAdminLanguageId);
				NotifySuccess(T("Report.Withdrawal.Completed"));
				return Json(new
				{
					Success = true
				});
			}
			catch(Exception ex)
			{
				return Json(new
				{
					Success = false
				});
			}
		}

		public void ReleaseLevelCommission(int planid, int customerid, float amountInvested)
		{
			var plancommission = _planService.GetPlanCommissionPlanId(planid).OrderBy(x => x.LevelId);
			Customer levelcustomer = _customerService.GetCustomerById(customerid);
			float commission = 0;
			try
			{
				foreach (var pc in plancommission)
				{
					commission = amountInvested * pc.CommissionPercentage / 100;
					levelcustomer = _customerService.GetCustomerById(levelcustomer.AffiliateId);
					if(levelcustomer != null)
					{
						var amountinvested = levelcustomer.Transaction.Where(x => x.CustomerId == levelcustomer.Id && x.StatusId == 2 && x.TranscationTypeId == 1).Sum(x => x.Amount);
						if (amountinvested > 0)
						{
							Transaction transaction = new Transaction();
							transaction.CustomerId = levelcustomer.Id;
							transaction.Amount = commission;
							transaction.FinalAmount = commission;
							transaction.TransactionDate = DateTime.Now;
							transaction.StatusId = (int)Status.Completed;
							transaction.TranscationTypeId = (int)TransactionType.Commission;
							_transactionService.InsertTransaction(transaction);
						}
					}
				}
			}
			catch(Exception ex)
			{

			}
		}
		[HttpPost]
		public ActionResult RejectRequest(int Id)
		{
			try
			{
				var transaction = _transactionService.GetTransactionById(Id);
				transaction.Deleted = true;
				_transactionService.UpdateTransaction(transaction);
				NotifySuccess(T("Report.Withdrawal.Rejected"));
				return Json(new
				{
					Success = true
				});
			}
			catch (Exception ex)
			{
				return Json(new
				{
					Success = false
				});
			}
		}

		[HttpPost]
		public ActionResult PayNow(int Id)
		{
			try
			{
				var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
				var transaction = _transactionService.GetTransactionById(Id);
				if (transaction.ProcessorId == (int)PaymentMethod.CoinPayment)
				{
					var coinpaymentSettings = _services.Settings.LoadSetting<CoinPaymentSettings>(storeScope);
					var withdrawalSettings = _services.Settings.LoadSetting<WithdrawalSettings>(storeScope);
					CoinPayments coinPayments = new CoinPayments(coinpaymentSettings.CP_ApiSecretKey, coinpaymentSettings.CP_PublicKey);
					var param = new SortedList<string, string>();
					param.Add("amount", transaction.Amount.ToString());
					param.Add("add_tx_fee", "1");
					param.Add("currency", "BTC");
					param.Add("currency2", "USD");
					param.Add("address", transaction.WithdrawalAddress);
					if (coinpaymentSettings.CP_EmailConfirmationRequired)
						param.Add("auto_confirm", "1");
					param.Add("note", withdrawalSettings.PaymentNote);
					var result = coinPayments.CallAPI("create_withdrawal", param);
					if (result["error"] == "" || result["error"] == null)
					{
						transaction.StatusId = (int)Status.Completed;
						_transactionService.UpdateTransaction(transaction);
						NotifySuccess(T("Report.Withdrawal.Rejected"));
						return Json(new
						{
							Success = true
						});
					}
				}
				else
				{
					NotifySuccess(T("Report.Withdrawal.AutoPayNotImplemented"));
					return Json(new
					{
						Success = true
					});
				}

				NotifySuccess("Something went wrong");
				return Json(new
				{
					Success = false
				});
			}
			catch (Exception ex)
			{
				return Json(new
				{
					Success = false
				});
			}
		}
	}
}