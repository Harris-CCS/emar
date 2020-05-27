using System;
using System.Linq;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using PulseCheck.Data.Common.Database;
using PulseCheck.QCPR.Domain.Contract;
using PulseCheck.QCPR.Domain.Data;
using PulseCheck.QCPR.Logic.Bindings;
using PulseCheck.QCPR.Logic.Bindings.Harris.UCW.BLL.Bindings;
using PulseCheck.QCPR.Logic.Managers;

namespace PulseCheck.QCPR.Logic.Tests
{
    [TestClass]
    public class QcprManagerTests
    {
        private static readonly AutoFacQcprLogicRegistrations _autoFacQcprLogicRegistrations = new AutoFacQcprLogicRegistrations();
        private static string _jsonData = "{\"data\":{\"procedure\":[{\"code\":\"36055\",\"facility\":\"QuadraMed Medical Center\",\"interface\":\"ICtylenol\",\"name\":\"Acetaminophen\",\"product\":[{\"DDID\":\"283\",\"GPI\":\"64200010000310\",\"code\":\"301\",\"form\":\"tablet\",\"form_interface\":\"TABS\",\"name\":\"Acetaminophen 325 mg Tablet (301)\",\"route\":[{\"name\":\"by mouth\"}],\"strength\":\"325\"},{\"DDID\":\"286\",\"GPI\":\"64200010005205\",\"code\":\"304\",\"form\":\"suppository\",\"form_interface\":\"SUPP\",\"name\":\"Acetaminophen 120 mg Pediatric Suppository\",\"route\":[{\"name\":\"rectal\"}],\"strength\":\"120\"},{\"DDID\":\"289\",\"GPI\":\"64200010005220\",\"code\":\"416\",\"form\":\"suppository\",\"form_interface\":\"SUPP\",\"name\":\"Acetaminophen 650 mg Suppository\",\"route\":[{\"name\":\"rectal\"}],\"strength\":\"650\"},{\"DDID\":\"283\",\"GPI\":\"64200010000310\",\"code\":\"432\",\"form\":\"tablet\",\"form_interface\":\"TABS\",\"name\":\"Acetaminophen 325 mg Tablet (Bulk/100)\",\"route\":[{\"name\":\"by mouth\"}],\"strength\":\"325\"},{\"DDID\":\"276\",\"GPI\":\"64200010000115\",\"code\":\"544\",\"form\":\"tablet\",\"form_interface\":\"TABS\",\"interface\":\"250\",\"name\":\"Acetaminophen 500 mg Extra Strength Tablet\",\"route\":[{\"name\":\"by mouth\"}],\"strength\":\"500\"},{\"DDID\":\"282\",\"GPI\":\"64200010002010\",\"code\":\"17481\",\"concentration_name\":\"mg/mL\",\"form\":\"suspension\",\"form_interface\":\"SUSP\",\"name\":\"Acetaminophen 160 mg/5 mL Children's Suspension, 1\",\"route\":[{\"name\":\"by mouth\"},{\"name\":\"mouth/throat\"}],\"strength\":\"120\"},{\"DDID\":\"294\",\"GPI\":\"64200010000515\",\"code\":\"17519\",\"form\":\"chewable tablet\",\"form_interface\":\"CHEW\",\"name\":\"Acetaminophen 160 mg Chewable Tablet\",\"route\":[{\"name\":\"by mouth\"}],\"strength\":\"160\"},{\"DDID\":\"35104\",\"GPI\":\"64200010000505\",\"code\":\"17643\",\"concentration_name\":\"mg/mL\",\"form\":\"drop\",\"name\":\"Acetaminophen  80 mg/0.8 mL Infant Suspension, 15 \",\"route\":[{\"name\":\"by mouth\"}],\"strength\":\"15\"},{\"DDID\":\"288\",\"GPI\":\"64200010005215\",\"code\":\"17796\",\"form\":\"suppository\",\"form_interface\":\"SUPP\",\"name\":\"Acetaminophen 325 mg Suppository\",\"route\":[{\"name\":\"rectal\"}],\"strength\":\"325\"},{\"DDID\":\"35967\",\"GPI\":\"64200010000420\",\"code\":\"18082\",\"form\":\"extended release tablet\",\"form_interface\":\"TBCR\",\"name\":\"Acetaminophen 650 mg Extended Release Tablet\",\"route\":[{\"name\":\"by mouth\"}],\"strength\":\"650\"},{\"DDID\":\"24347\",\"GPI\":\"64200010005203\",\"code\":\"18487\",\"form\":\"suppository\",\"form_interface\":\"SUPP\",\"name\":\"Acetaminophen  80 mg Suppository\",\"route\":[{\"name\":\"rectal\"}],\"strength\":\"80\"},{\"DDID\":\"010691\",\"code\":\"18488\",\"concentration_name\":\"mg/mL\",\"form\":\"drop\",\"name\":\"Acetaminophen 100 mg/mL Drops, 15 mL\",\"route\":[{\"name\":\"by mouth\"}],\"strength\":\"15\"},{\"DDID\":\"276\",\"GPI\":\"64200010000115\",\"code\":\"18614\",\"form\":\"tablet\",\"form_interface\":\"TABS\",\"interface\":\"500\",\"name\":\"Acetaminophen 500 mg Extra Strength Tablet (slb)\",\"route\":[{\"name\":\"by mouth\"},{\"name\":\"xxxxas directed\"}],\"strength\":\"500\"},{\"DDID\":\"282\",\"GPI\":\"64200010002010\",\"code\":\"18749\",\"concentration_name\":\"mg/mL\",\"form\":\"syrup\",\"form_interface\":\"SYRP\",\"name\":\"CarbonLazypine(DEG) 300 mg/15 mL Oral Syrup, 240 m\",\"route\":[{\"name\":\"by mouth\"},{\"name\":\"tube - nasal gastric\"}],\"strength\":\"240\"},{\"DDID\":\"35967\",\"GPI\":\"64200010000420\",\"code\":\"634\",\"form\":\"tablet\",\"form_interface\":\"TABS\",\"name\":\"Acetaminophen 650 mg Tablet\",\"route\":[{\"name\":\"by mouth\"}],\"strength\":\"650\"},{\"DDID\":\"022584\",\"code\":\"17623\",\"form\":\"tablet\",\"form_interface\":\"TABS\",\"name\":\"Tylenol Junior 160 mg Tablet\",\"route\":[{\"name\":\"by mouth\"}],\"strength\":\"160\"},{\"DDID\":\"22566\",\"GPI\":\"64200010000310\",\"code\":\"17524\",\"form\":\"tablet\",\"form_interface\":\"TABS\",\"interface\":\"TEST Code\",\"name\":\"Tylenol 325 mg Tablet\",\"route\":[{\"name\":\"by mouth\"},{\"name\":\"oral\"}],\"strength\":\"325\"},{\"code\":\"18971\",\"form\":\"tablet\",\"form_interface\":\"TABS\",\"interface\":\"TEST Code\",\"name\":\"Tylenol 325 mg Tablet (TEST)\",\"route\":[{\"name\":\"by mouth\"}],\"strength\":\"800\"},{\"code\":\"17752\",\"form\":\"capsule\",\"form_interface\":\"CAPS\",\"interface\":\"if\",\"name\":\"Acetaminophen 250 mg Capsule {SEI}\",\"route\":[{\"name\":\"by mouth\"}],\"strength\":\"250\"},{\"DDID\":\"174835\",\"GPI\":\"64200010000320\",\"code\":\"18999\",\"form\":\"tablet\",\"form_interface\":\"TABS\",\"name\":\"Acetaminophen 5 grain Tablet\",\"route\":[{\"name\":\"by mouth\"}],\"strength\":\"5\"},{\"code\":\"19017\",\"form\":\"suppository\",\"form_interface\":\"SUPP\",\"name\":\"CCT Acetaminophen  80 mg Suppository\",\"route\":[{\"name\":\"rectal\"}],\"strength\":\"80\"},{\"code\":\"19025\",\"form\":\"tablet\",\"form_interface\":\"TABS\",\"name\":\"Acetaminophen 5 grain Tablet (copy)\",\"route\":[{\"name\":\"by mouth\"}],\"strength\":\"5\"},{\"DDID\":\"282\",\"GPI\":\"64200010002010\",\"code\":\"19066\",\"concentration_name\":\"mg/mL\",\"form\":\"suspension\",\"form_interface\":\"SUSP\",\"name\":\"Acetaminophen 160 mg/5 mL Children's Suspension, 1\",\"route\":[{\"name\":\"by mouth\"},{\"name\":\"mouth/throat\"}],\"strength\":\"10\"},{\"DDID\":\"22566\",\"GPI\":\"64200010000310\",\"code\":\"19081\",\"form\":\"tablet\",\"form_interface\":\"TABS\",\"interface\":\"TEST Code\",\"name\":\"Tylenol 325 mg Tablet (copy)\",\"route\":[{\"name\":\"by mouth\"}],\"strength\":\"325\"},{\"DDID\":\"283\",\"GPI\":\"64200010000310\",\"code\":\"19092\",\"form\":\"tablet\",\"form_interface\":\"TABS\",\"name\":\"Acetaminophen 325 mg Tablet (PXJ)\",\"route\":[{\"name\":\"by mouth\"}],\"strength\":\"325\"},{\"DDID\":\"22566\",\"GPI\":\"64200010000310\",\"code\":\"19136\",\"form\":\"tablet\",\"form_interface\":\"TABS\",\"interface\":\"TEST Code\",\"name\":\"Tylenol 325 mg Tablet (copy)\",\"route\":[{\"name\":\"by mouth\"}],\"strength\":\"325\"},{\"DDID\":\"286\",\"GPI\":\"64200010005205\",\"code\":\"18747\",\"form\":\"suppository\",\"form_interface\":\"SUPP\",\"interface\":\"tst\",\"name\":\"AcetaCinomen(DEG) 220 mg Geriatric Suppository\",\"route\":[{\"name\":\"rectal\"}],\"strength\":\"220\"}]},{\"code\":\"36598\",\"facility\":\"QuadraMed Medical Center\",\"name\":\"Vancomycin Hydrochloride\",\"product\":[{\"DDID\":\"23107\",\"GPI\":\"16000060102108\",\"code\":\"599\",\"form\":\"injection\",\"form_interface\":\"INJ\",\"name\":\"Vancomycin HCl 1 g Injection, 20 mL Vial\",\"route\":[{\"name\":\"IV piggyback\"},{\"name\":\"intravenous\"},{\"name\":\"IV piggyback (cart)\"}],\"strength\":\"1\"},{\"DDID\":\"023099\",\"code\":\"601\",\"form\":\"solution\",\"name\":\"Vancomycin HCl  500 mg/6 mL Oral Solution, 10 g\",\"route\":[{\"name\":\"by mouth\"}],\"strength\":\"10\"},{\"DDID\":\"23107\",\"GPI\":\"16000060102108\",\"code\":\"602\",\"form\":\"injection\",\"form_interface\":\"INJ\",\"name\":\"Vancomycin HCl 1 gm Injection, Vial\",\"route\":[{\"name\":\"IV piggyback\"},{\"name\":\"intravenous\"},{\"name\":\"IV piggyback (cart)\"}],\"strength\":\"1\"},{\"DDID\":\"23103\",\"GPI\":\"16000060100120\",\"code\":\"603\",\"form\":\"capsule\",\"form_interface\":\"CAPS\",\"name\":\"Vancomycin HCl  250 mg Capsule\",\"route\":[{\"name\":\"by mouth\"}],\"strength\":\"250\"},{\"code\":\"604\",\"concentration_name\":\"mg/mL\",\"form\":\"solution\",\"name\":\"Vancomycin HCl  500 mg/10 mL Solution, 1 g\",\"route\":[{\"name\":\"by mouth\"}],\"strength\":\"1\"},{\"code\":\"18631\",\"name\":\"Vancomycin 1 g in D5W, 250 mL Premix (IMM)\"},{\"DDID\":\"23109\",\"GPI\":\"16000060102109\",\"code\":\"18830\",\"form\":\"injection\",\"form_interface\":\"INJ\",\"name\":\"Vancomycin HCl 5 gm Injection, Vial\",\"route\":[{\"name\":\"IV piggyback\"},{\"name\":\"intravenous\"},{\"name\":\"IV piggyback (cart)\"}],\"strength\":\"5\"},{\"code\":\"18860\",\"name\":\"Vancomycin 500 mg/100 mL D5W Injection (IMM mL DKV\"}]}]},\"status\":{\"code\":0}}";
        private static IQcprImportData _importData;
        private static IQcprManager _qcprManager;
        private static IDbConnectionSettings _ibexArchiveConnectionSettings;

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            AutoMapperRegistrationSingleton.Register();

            _autoFacQcprLogicRegistrations.LoadContainer();
            _importData = JsonConvert.DeserializeObject<QcprImportData>(_jsonData);
            _qcprManager = _autoFacQcprLogicRegistrations.GetType<IQcprManager>();
        }

        [TestMethod]
        public void Init_ImportData_NotNull()
        {
            Assert.IsNotNull(_importData);
            Assert.IsNotNull(_importData.data);
            Assert.IsNotNull(_importData.status);
        }

        [TestMethod]
        public void Init_QcprManager_NotNull()
        {
            Assert.IsNotNull(_qcprManager);
        }

        [TestMethod]
        public void SaveImportData_Json_Success()
        {
            _qcprManager.SaveImportData(_jsonData);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void SaveImportData_Json_Null()
        {
            _qcprManager.SaveImportData(json: null);
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [TestMethod]
        public void SaveImportData_Json_Invalid()
        {
            _qcprManager.SaveImportData(_jsonData.Substring(5));
        }


        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void SaveImportData_ImportData_Null()
        {
            _qcprManager.SaveImportData(importData: null);
        }

        //[TestMethod]
        //public void SaveImportData_ImportData_Success()
        //{
        //    _qcprManager.SaveImportData(_importData);
        //}


        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void SaveImportData_ImportData_Data_Null()
        {
            QcprImportData data = new QcprImportData();
            data.status = _importData.status;
            _qcprManager.SaveImportData(data);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void SaveImportData_ImportData_Status_Null()
        {
            QcprImportData data = new QcprImportData();
            data.data = _importData.data;
            _qcprManager.SaveImportData(data);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public void SaveImportData_ImportData_Status_Code_NotZero()
        {
            IQcprImportData data = _importData;
            data.status.code = 1;
            _qcprManager.SaveImportData(data);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void GetProductsByName_Param_Name_Null()
        {
            _qcprManager.GetProductsByName(null);
        }

        [TestMethod]
        public void GetProductsByName_Param_Name_Success()
        {
            var start = DateTime.Now;
            var result = _qcprManager.GetProductsByName("Aceta");
            var end = DateTime.Now;
            Console.WriteLine(end - start);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        //[TestMethod]
        //public void GetProductsById_Param_Id_Success()
        //{
        //    var start = DateTime.Now;
        //    var result = _qcprManager.GetProductsById(74262);
        //    var end = DateTime.Now;
        //    Console.WriteLine(end - start);
        //    Assert.IsNotNull(result);
        //    Assert.IsTrue(result.Any());
        //}

        [TestMethod]
        public void GetProceduresByName_Param_Name_Success()
        {
            var result = _qcprManager.GetProceduresByName("Aceta");
            Assert.IsNotNull(result);
        }

        //[TestMethod]
        //public void GetVendorQcprData()
        //{
        //    var result = _qcprManager.GetQcprJsonFromVendor();
        //    Assert.IsNotNull(result);
        //    Assert.IsTrue(result.Length > 0);
        //}


    }
}
