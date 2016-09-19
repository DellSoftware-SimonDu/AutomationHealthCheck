using System;


namespace Dell.WebTests
{
    public class Constant
    {
        public const String OBJECT_URL = "https://test.spotlightessentials.com";
        public const String USER_NAME = "ZhuHaiWebUi.Test@gmail.com";
        public const String PASSWORD = "Quest123";

        public const string UPDATE_SAMPLE_DATE = "powershell -file C:\\Jenkins\\workspace\\spotlight-web--test--spotlight-web-us1-test\\WebTests\\UploadSampleData\\UploadData.ps1";
        public const string DELETE_DS = "powershell -file C:\\Jenkins\\workspace\\spotlight-web--test--spotlight-web-us1-test\\WebTests\\UploadData\\DeleteDS.ps1";

        public const String AG_CONNCTION_NAME = "ZHUW2K12SPLHAL.melquest.dev.mel.au.qsft";
        public const String DS_CONNCTION_NAME = "win2012GA116A:3843";
        public const String HPV_CONNCTION_NAME = "melrdhypervw01.melquest.dev.mel.au.qsft";
        public const String REP_CONNCTION_NAME = "zhuw2k8r2spl300.melquest.dev.mel.au.qsft\\sql2008r2";
        public const String AZ_CONNCTION_NAME = "sossesqlazure.database.windows.net\\SoSSESQLAzure";
        public const String VM_CONNCTION_NAME = "zhuvcradw10.prod.quest.corp";
        public static String[] winConnNames = {"wz64w2008sql.melquest.dev.mel.au.qsft",
                                       "ZHUW12R2SPL303.melquest.dev.mel.au.qsft",
                                       "ZHUW12R2SPL501",
                                       "ZHUW2K12SPL007.melquest.dev.mel.au.qsft",
                                       "zhuw2k8r2spl300.melquest.dev.mel.au.qsft"
                                        };
        public static String[] sqlConnNames ={"wz64w2008sql.melquest.dev.mel.au.qsft\\SOSSE",
                                     "ZHU12708.prod.quest.corp\\SQL2012",
                                     "ZHUW12R2SPL404.melquest.dev.mel.au.qsft\\SQL2016",
                                     "ZHUW12R2SPL501\\sql2014",
                                     "ZHUW12R2SPL501\\sql2016",
                                     "ZHUW2K12SPL007.melquest.dev.mel.au.qsft\\ZHUSPLSQL2K14N02",
                                     "zhuw2k8r2spl300.melquest.dev.mel.au.qsft\\sql2008r2",
                                     "zhuw2k8spl004.melquest.dev.mel.au.qsft\\zhusplsql2005n1"
                                      };
        public static String[] sqlAlarmTypes = {"HasHighSeverityAlarms",
                                         "HasLowSeverityAlarms",
                                         "Disabled"
                                         };

        public static int[] sqlAlarmCounts = { 6, 1, 1 };

        public static String[] winAlarmTypes = {"HasHighSeverityAlarms",
                                         "HasMediumSeverityAlarms",
                                         "HasLowSeverityAlarms",
                                         "NoAlarms"
                                         };
        public static int[] winAlarmCounts = { 1, 2, 1, 1 };

        public static String[] singleConnAlarmSeverity = { "High", "High", "High", "High", "Low", "Information" };
        public static String[] singleTypeAlarmSeverity = { "High", "Low", "Medium", "Medium", "Low", "Low" };
        public static String[] singleTypeConnNames = {
                                                 "ZHUW2K12SPL007.melquest.dev.mel.au.qsft",
                                                 "wz64w2008sql.melquest.dev.mel.au.qsft",
                                                 "zhuw2k8r2spl300.melquest.dev.mel.au.qsft",
                                                 "ZHUW12R2SPL303.melquest.dev.mel.au.qsft"
                                                 };

        public static String[] allSortTypeNames = {
                            "ZHUW12R2SPL501\\sql2016",
                            "zhuw2k8r2spl300.melquest.dev.mel.au.qsft\\sql2008r2",
                            "ZHU12708.prod.quest.corp\\SQL2012",
                            "ZHUW2K12SPL007.melquest.dev.mel.au.qsft",
                            "wz64w2008sql.melquest.dev.mel.au.qsft\\SOSSE",
                            "ZHUW2K12SPL007.melquest.dev.mel.au.qsft\\ZHUSPLSQL2K14N02",
                            "ZHUW12R2SPL404.melquest.dev.mel.au.qsft\\SQL2016",
                            "zhuw2k8spl004.melquest.dev.mel.au.qsft\\zhusplsql2005n1",
                            "zhuw2k8r2spl300.melquest.dev.mel.au.qsft\\sql2008r2",
                            "ZHUW2K12SPLHAL.melquest.dev.mel.au.qsft",
                            "wz64w2008sql.melquest.dev.mel.au.qsft",
                            "zhuw2k8r2spl300.melquest.dev.mel.au.qsft",
                            "ZHUW12R2SPL501\\sql2014",
                            "ZHUW12R2SPL303.melquest.dev.mel.au.qsft"
                         };

        public static String[] afterRefreshAlarmTypes = { "HasLowSeverityAlarms", "NoAlarms" };
        public static String[] preRefreshAlarmTypes = { "HasHighSeverityAlarms", "NoAlarms" };
    }
}
