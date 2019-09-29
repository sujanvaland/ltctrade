using SmartStore.Services.Hyip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Domain.Hyip;
using SmartStore.Services;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Core;
using SmartStore.Services.Customers;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services.Localization;
using SmartStore.Core.Domain.Localization;
using SmartStore.Services.Boards;

namespace SmartStore.Web.Controllers
{
    public class IPNHandlerController : PublicControllerBase
	{
		private readonly ICustomerPlanService _customerPlanService;
		private readonly ITransactionService _transactionService;
		private readonly ICommonServices _commonService;
		private readonly IStoreContext _storeContext;
		private readonly IPlanService _planService;
		private readonly ICustomerService _customerService;
		private readonly LocalizationSettings _localizationSettings;
		private readonly IBoardService _boardService;
		public IPNHandlerController(ICustomerPlanService customerPlanService,
			ITransactionService transactionService,
			ICommonServices commonService,
			IStoreContext storeContext,
			IPlanService planService,
			ICustomerService customerService,
			LocalizationSettings localizationSettings,
			IBoardService boardService)
		{
			_customerPlanService = customerPlanService;
			_transactionService = transactionService;
			_commonService = commonService;
			_storeContext = storeContext;
			_planService = planService;
			_customerService = customerService;
			_localizationSettings = localizationSettings;
			_boardService = boardService;
		}
		static string ByteToString(byte[] buff)
		{
			string sbinary = "";
			for (int i = 0; i < buff.Length; i++)
				sbinary += buff[i].ToString("X2"); /* hex format */
			return sbinary;
		}
		// GET: IPNHandler
		public ActionResult CoinpaymentIPN()
        {
			var coinpaymentSettings = _commonService.Settings.LoadSetting<CoinPaymentSettings>(_storeContext.CurrentStore.Id);
			string MerchantId = coinpaymentSettings.CP_MerchantId;
			string Secret = coinpaymentSettings.CP_SecretKey;

			if(Request.ServerVariables["HTTP_HMAC"] == null)
			{
				return Content("HTTP_HMAC not found");
			}
			if (Request.ServerVariables["HTTP_HMAC"].ToString() == "")
			{
				return Content("HTTP_HMAC not found");
			}

			string serverMerchantId = (Request["merchant"] == null) ? "" : Request["merchant"].ToString();
			if(serverMerchantId == MerchantId)
			{
				if (int.Parse(Request["status"].ToString()) >= 100)
				{
					var transaction = _transactionService.GetTransactionById(int.Parse(Request["custom"].ToString()));
					if (transaction.StatusId != 2)
					{
						transaction.Status = Status.Completed;
						transaction.StatusId = (int)Status.Completed;
						transaction.FinalAmount = transaction.Amount;
						_transactionService.UpdateTransaction(transaction);
						Services.MessageFactory.SendDepositNotificationMessageToUser(transaction, "", "", _localizationSettings.DefaultAdminLanguageId);
						Services.MessageFactory.SendDepositNotificationMessageToAdmin(transaction, "", "", _localizationSettings.DefaultAdminLanguageId);
					}
				}
			}
			
			return Content("MerchantId not matched");
		}

		public void ApproveTransaction(int transid)
		{
			var transaction = _transactionService.GetTransactionById(transid);
			if (transaction.StatusId != 2)
			{
				transaction.Status = Status.Completed;
				transaction.StatusId = (int)Status.Completed;
				transaction.FinalAmount = transaction.Amount;
				_transactionService.UpdateTransaction(transaction);
				Services.MessageFactory.SendDepositNotificationMessageToUser(transaction, "", "", _localizationSettings.DefaultAdminLanguageId);
			}
		}

		public void WritetoLog(string message)
		{
			System.IO.File.WriteAllText(Server.MapPath("/WriteLines.txt"),message);
		}
	}
}