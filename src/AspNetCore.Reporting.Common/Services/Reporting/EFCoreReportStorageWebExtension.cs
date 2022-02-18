﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using AspNetCore.Reporting.Common.Data;
using AspNetCore.Reporting.Common.Models;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.Extensions;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Reporting.Common.Services.Reporting {
    public class EFCoreReportStorageWebExtension<T> : ReportStorageWebExtension where T : DbContext, IReportEntityProvider, IStudentEntityProvider {
        private readonly IAuthenticatiedUserService userService;
        private readonly T dBContext;

        public EFCoreReportStorageWebExtension(IAuthenticatiedUserService userService, T dBContext) {
            this.userService = userService;
            this.dBContext = dBContext;
        }

        public override bool CanSetData(string url) {
            return true;
        }

        public override bool IsValidUrl(string url) {
            return true;
        }

        public override byte[] GetData(string url) {
            var userIdentity = userService.GetCurrentUserId();
            var reportData = dBContext.Reports.Where(a => a.ID == int.Parse(url) && a.Student.Id == userIdentity).FirstOrDefault();
            if(reportData != null) {
                return reportData.ReportLayout;
            } else {
                throw new DevExpress.XtraReports.Web.ClientControls.FaultException(string.Format("Could not find report '{0}'.", url));
            }
        }

        public override Dictionary<string, string> GetUrls() {
            var userIdentity = userService.GetCurrentUserId();
            var reportData = dBContext.Reports.Where(a => a.Student.Id == userIdentity).Select(a => new ReportingControlModel() { Id = a.ID.ToString(), Title = string.IsNullOrEmpty(a.DisplayName) ? "Noname Report" : a.DisplayName });
            var reports = reportData.ToList();
            return reports.ToDictionary(x => x.Id.ToString(), y => y.Title);
        }

        public override void SetData(XtraReport report, string url) {
            var userIdentity = userService.GetCurrentUserId();
            var reportEntity = dBContext.Reports.Where(a => a.ID == int.Parse(url) && a.Student.Id == userIdentity).FirstOrDefault();
            reportEntity.ReportLayout = ReportToByteArray(report);
            reportEntity.DisplayName = report.DisplayName;
            dBContext.SaveChanges();
        }

        public override string SetNewData(XtraReport report, string defaultUrl) {
            var userIdentity = userService.GetCurrentUserId();
            var user = dBContext.Students.Find(userIdentity);
            var newReport = new ReportEntity() { DisplayName = defaultUrl, ReportLayout = ReportToByteArray(report), Student = user };
            dBContext.Reports.Add(newReport);
            dBContext.SaveChanges();
            return newReport.ID.ToString();
        }

        static byte[] ReportToByteArray(XtraReport report) {
            using(var memoryStream = new MemoryStream()) {
                report.SaveLayoutToXml(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
