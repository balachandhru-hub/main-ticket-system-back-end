
namespace Entities.Common
{
        /// <summary>
        ///
        /// </summary>
        public static class Common
        {
                public static readonly string APPLICATION_SCHEMA = "ConnectionStrings:Schema";
                public static readonly string TOKEN_KEY = "Tokens:key";
                public static readonly string TOKEN_EXPIRY = "Tokens:TokenExpirationTimeInSeconds";
                public static readonly string BASE_FOLDER_PATH = "FolderPath:BasePath";
                public static readonly string ASSET_FOLDER_PATH = "asset";
                public const string TOKEN_ISSUER = "Tokens:Issuer";
                public static int TOKEN_EXPIRY_TIME_DEFAULT = 1200;
                public static Guid DEFAULT_ROLE_ID = new Guid("f0db8007-a05a-4657-bc3e-5638c0d57fc5");
                public static string INITIAL_STATUS_FOR_TICKET = "OPEN";
                public static Guid ADMIN_ROLE_ID = new Guid("9c9955a6-2246-4cce-abe8-e2183c14f7cb");
                public static Guid BUSINESS_USER_ROLE_ID = new Guid("f0db8007-a05a-4657-bc3e-5638c0d57fc5");
                public static Guid BUSINESS_ADMIN_ROLE_ID = new Guid("b7f623f7-9564-4031-9a4a-11ab1fac81d6");
                public static Guid MANAGER_ROLE_ID = new Guid("7fa817e2-b8ab-4e28-8a5b-09ca3aefd108");
                public static Guid CONSULTANT_ROLE_ID = new Guid("1f18e16a-826a-4ba5-969c-536b0963edf5");
                public static Guid LEAD_ROLE_ID = new Guid("e29577d6-4213-438b-9f80-5d643713a1ee");
                public static Guid LEADERSHIP_USER_ROLE_ID = new Guid("0e916217-9f4d-4371-b153-6c9deb5a6c95");
                public static Guid OPERATIONS_USER_ROLE_ID = new Guid("cda1c1e5-d858-4403-8f59-1a61921591e9");
                public static Guid FINANCE_OPERATION_USER_ROLE_ID = new Guid("0b0e9172-8d32-485c-9556-927f0fd63347");
                public static string TICKET_MAX_CREATION_VALUE_KEY = "MAX_TICKET_CREATE";
                public static readonly string REFRESH_TOKEN_EXPIRY = "Tokens:RefershTokenExpirationTimeInSeconds";
                public static int REFRESH_TOKEN_EXPIRY_TIME_DEFAULT = 1200;
                public static string MESSAGE_TYPE_KEY = "MESSAGE";
                public static string ASSET_TYPE_KEY = "ASSET";
                public static string ENTITY_TYPE_TICKET = "TICKET";
                public static string STATUS_APPROVE = "APPROVED";
                public static string STATUS_CONTRACT_APPROVE = "Approve";
                public static string STATUS_CONTRACT_DENY = "Deny";
                public static string STATUS_CLOSED = "CLOSED";
                public static string STATUS_RE_APPROVE = "RE_APPROVE";
                public static string STATUS_IN_PROGRESS = "IN_PROGRESS";
                public static string STATUS_ON_HOLD = "ON_HOLD";
                public static string STATUS_COMPLETED = "RESOLVED";
                public static string STATUS_DECLINE = "DECLINE";
                public static string STATUS_DENY = "deny";
                public static readonly string TO_EMAIL_PLACEHOLDER = "{toEmail}";
                public static readonly string SUBJECT_PLACEHOLDER = "{subject}";
                public static readonly string EMAIL_BODY_PLACEHOLDER = "{emailBody}";
                public static readonly string EMAIL_TYPE_PLACEHOLDER = "{emailType}";
                public static readonly string SENDER_DISPLAYNAME = "EmailSettings:Displayname";
                public static readonly string SENDER_PORT = "EmailSettings:Port";
                public static readonly string EMAIL_API_CREDENTIAL_DESCRIPTION = "EmailCredentials";
                public static readonly string SCHEDULER_HOUR = "EmailSettings:SchedulerHour";
                public static readonly string SCHEDULER_MINUTE = "EmailSettings:SchedulerMinute";
                public static readonly string SCHEDULER_COUNT = "EmailSettings:SchedulerCount";
                public static readonly string BUSINESS_ROLE_CATEGORY_KEY = "BUSINESS_ROLE_CATEGORY";
                public static readonly string GLOBAL_ROLE_CATEGORY_KEY = "GLOBAL_ROLE_CATEGORY";
                public static readonly string BUSINESS_ADMIN_USER_KEY = "BUSINESS_ADMIN_USER";
                public static readonly string BUSINESS_USER_KEY = "BUSINESS_USER";
                public static readonly string LEADERSHIP_USER_KEY = "LEADERSHIP_USER";
                public static readonly string SUPPLIER_USER_KEY = "SUPPLIER_USER";
                public static readonly string OPERATIONS_USER_KEY = "OPERATIONS_USER";
                public static readonly string FINANCE_OPERATION_USER_KEY = "FINANCE_OPERATION_USER";


                public static readonly Guid GLOBAL_DOMAIN_ID = Guid.Parse("9db82795-043e-43ca-b4bf-ee9cb16967a3");
                public static readonly string SUBSCRIPTION_PLAN_KEY = "SUBSCRIPTION";
                public static readonly string SUPPORT_TEAM_PLAN_KEY = "SUPPORT_TEAM";
                public static readonly string PAY_AS_USE = "PAY_AS_USE";
                public const float DEFAULT_TICKET_HOURS = 8;
                public static readonly string ENTITY_TYPE_USER = "USER";
                public static string INITIAL_STATUS_FOR_ADDITIONAL_QUERY = "OPEN";
                public const string ENGAGEMENT_TYPE = "ENGAGEMENT_TYPE";
                public const string CHARGE_PER_TICKET = "CHARGE_PER_TICKET";
                public const string DISCOUNT_CODE = "DISCOUNT_CODE";
                public const string CR_BEYOND_8_HOURS = "CR_BEYOND_8_HOURS";
                public const string MONTHLY_HOURS = "MONTHLY_HOURS";
                public const string PAYMENT_TYPE = "PAYMENT_TYPE";
                public const string RESOURCE_COST = "RESOURCE_COST";
                public const string NO_OF_CONSULTANTS = "NO_OF_CONSULTANTS";
                public const string CR_CHARGES_FOR_ADDITIONAL_HOURS = "CR_CHARGES_FOR_ADDITIONAL_HOURS";
                public const string MANAGER = "MANAGER";
                public const string RESOURCES = "RESOURCES";
                public const string CR_HOURS = "CR_HOURS";
                public const string MANAGER_COST = "MANAGER_COST";
                public static readonly Guid DEFAULT_DOMAIN_ID = Guid.Parse("9db82795-043e-43ca-b4bf-ee9cb16967a3");
                public static readonly string DEFAULT_TICKET_HOURS_BUCKET = "LESSTHAN_8_HOURS";
                public static readonly string COOKIE_ACCESS_TOKEN_KEY = "access_token";
                public static readonly string COOKIE_REFRESH_TOKEN_KEY = "refresh_token";
                public static readonly string DEFAULT_FRONT_END_ORIGIN = "Origin:HostOrigin";
                public static readonly string DEFAULT_FRONT_END_ORIGIN_LOCAL = "Origin:HostOriginLocal";
                public static readonly string DEFAULT_FRONT_END_ORIGIN_LOCAL_SINGLE_APP = "Origin:HostOriginLocalSingleApp";
                public static readonly string DEFAULT_FRONT_END_ORIGIN_SINGLE_APP = "Origin:HostOriginSingleApp";
                public static readonly string TICKET_DECLINE_STATUS_KEY = "DECLINE";
                public static readonly Guid UPLOADED_FILE_TYPE_ID = new Guid("6b71eb95-c3a8-418c-88a7-502068fe11ae");
                public static readonly string UPLOADED_FILE = "UPLOADED_FILE";
                public static readonly Guid TICKET_MESSAGE_ASSET_TYPE_ID = Guid.Parse("eff83cb9-94d8-4211-a100-24f9ea632646");
                public static readonly Guid UPLOADED_FILE_ID = Guid.Parse("6b71eb95-c3a8-418c-88a7-502068fe11ae");
                public static readonly string UNPAID_PAYMENT_STATUS = "UNPAID";
                public static readonly string PARTIAL_PAYMENT_STATUS = "PARTIAL_PAID";
                public static readonly string RE_APPROVE_STATUS = "RE_APPROVE";
                public static readonly string DOMAIN_LOGO = "DOMAIN_LOGO";
                public static readonly Guid DOMAIN_ENTITY_TYPE_ID = new Guid("302b61a8-85f3-4893-8385-1147b2532caa");
                public static readonly Guid DOMAIN_LOGO_ASSET_TYPE_ID = new Guid("c64eb07e-1c98-49e9-b847-f52d84ce8124");
                public static readonly string PAID_PAYMENT_STATUS = "PAID";
                public static readonly string PENDING_PAYMENT_STATUS = "PENDING";
                public static readonly string ENTITY_TYPE_DOMAIN_PLAN = "DOMAIN_PLAN";
                public static readonly string ASSET_TYPE = "CONTRACT_FILE";

                public static readonly string REJECTED_TICKET_STATUS = "REJECTED";

                public static readonly string ACTIVE_STATUS = "ACTIVE";
                public static readonly string INACTIVE_STATUS = "INACTIVE";
                public static readonly string ARIBA_DOMAIN_ORIGIN = "Origin:AribaDomainOrigin";
                public const string COUPON_CODE_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                public const int COUPON_CODE_LENGTH = 20;
                public const int DISCOUNT_MIN = 1;
                public const int DISCOUNT_MAX = 50;
                public const string DISCOUNT_MIN_KEY = "DISCOUNT_MIN";
                public const string DISCOUNT_MAX_KEY = "DISCOUNT_MAX";
                public static readonly string CONSULTANT_ROLE = "CONSULTANT";
                public static readonly string MONTHLY = "MONTHLY";
                public static readonly string YEARLY = "YEARLY";
                public const string ARIBA_MODEL = "ARIBA_MODEL";
                public static readonly string PERSON_NAME_EMAIL_PLACEHOLDER = "{USER_NAME}";
                public static readonly string TICKET_ID_EMAIL_PLACEHOLDER = "{TICKET_ID}";
                public static readonly string DATE_PLACEHOLDER = "{DATE}";
                public static readonly string TITLE_PLACEHOLDER = "{TITLE}";
                public static readonly string DESCRIPTION_PLACEHOLDER = "{DESCRIPTION}";
                public static readonly string COMPANY_PLACEHOLDER = "{COMPANY_NAME}";
                public static readonly string SUPPORT_EMAIL_PLACEHOLDER = "{SUPPORT_EMAIL}";
                public static string ENTITY_TYPE_DOMAIN = "DOMAIN";
                public static readonly string COMPANY_LOGO_URL_PLACEHOLDER = "{CAS_LOGO_URL}";
                public static readonly string COMPANY_LOGO_URL = "EmailSettings:CompanyLogo";
                public static readonly string SUPPORT_EMAIL = "EmailSettings:SupportEmail";
                public static readonly string COMPANY_NAME = "EmailSettings:CompanyName";
                public static readonly string PORTAL_LINK = "EmailSettings:PortalLink";
                public static readonly string PORTAL_LINK_PLACEHOLDER = "{SUPPORT_PORTAL_LINK}";
                public static readonly string TICKET_CREATION_EMAIL_KEY = "TICKET_CREATION";
                public static readonly string TICKET_MODIFICATION_EMAIL_KEY = "TICKET_MODIFICATION";

                public static readonly string DOMAIN_CREATION_EMAIL_KEY = "DOMAIN_CREATION";
                public static readonly string DOMAIN_COMPANY_NAME_PLACEHOLDER = "{DOMAIN_COMPANY_NAME}";
                public static readonly string DOMAIN_WEBSITE_URL_PLACEHOLDER = "{WEBSITE_URL}";
                public static readonly string DOMAIN_COMPANY_LOCATION_PLACEHOLDER = "{COMPANY_LOCATION}";
                public static readonly string DOMAIN_CIN_NO_PLACEHOLDER = "{CIN_NO}";
                public static readonly string DOMAIN_COUNTRY_PLACEHOLDER = "{COUNTRY}";
                public static readonly string DOMAIN_BUSINESS_TYPE_PLACEHOLDER = "{BUSINESS_TYPE}";
                public static readonly string DOMAIN_INDUSTRY_TYPE_PLACEHOLDER = "{INDUSTRY_TYPE}";
                public static readonly string DOMAIN_BILLING_ADDR_PLACEHOLDER = "{BILLING_ADDR}";
                public static readonly string USER_CREATION_EMAIL_KEY = "USER_CREATION";
                public static readonly string USERNAME_PLACEHOLDER = "{USER_NAME}";
                public static readonly string USERSECRET_PLACEHOLDER = "{USER_SECRET}";


                public static readonly string DOMAIN_PLAN_CREATION_EMAIL_KEY = "DOMAIN_PLAN_CREATION";
                public static readonly string DOMAIN_PLAN_NAME_PLACEHOLDER = "{PLAN_NAME}";
                public static readonly string DOMAIN_PLAN_ENGAGE_TYPE_PLACEHOLDER = "{PLAN_ENGAGE_TYPE}";
                public static readonly string DOMAIN_PLAN_START_DATE_PLACEHOLDER = "{PLAN_START_DATE}";
                public static readonly string DOMAIN_PLAN_END_DATE_PLACEHOLDER = "{PLAN_END_DATE}";
                public static readonly string DOMAIN_PLAN_PAYMENT_TYPE_PLACEHOLDER = "{PLAN_PAYMENT_TYPE}";
                public static readonly string DOMAIN_DIS_COUP_ID_PLACEHOLDER = "{DIS_COUP_ID}";
                public static readonly string DOMAIN_CONTRACT_START_DATE_PLACEHOLDER = "{CONTRACT_START_DATE}";
                public static readonly string DOMAIN_CONTRACT_END_DATE_PLACEHOLDER = "{CONTRACT_END_DATE}";
                public static readonly string DOMAIN_NUMBER_OF_DUES_PLACEHOLDER = "{NUMBER_OF_DUES}";

                public static readonly string DOMAIN_SETTINGS_ARIBA_PURCHASE_ORDER_CONFIG = "ARIBA_PURCHASE_ORDER_CONFIG";
                public static readonly string DOMAIN_SETTINGS_ARIBA_TOKEN = "ARIBA_TOKEN";
                public static readonly string DOMAIN_SETTINGS_ARIBA_INVOICE = "ARIBA_INVOICE";
                public static readonly string DOMAIN_SETTINGS_CONTRACT_CONFIG = "CONTRACT_CONFIG";
                public static readonly string DOMAIN_SETTINGS_ARIBA_CONTRACT_URL = "ARIBA_CONTRACT_URL";
                public static readonly string DOMAIN_SETTINGS_SUPPLIER_FETCH_TOKEN_AND_URL = "SUPPLIER_FETCH_TOKEN_AND_URL";

                // schedular Keys
                public static readonly string SCHEDULAR_KEY_GET_FACT_RECIEPTS = "GET_FACT_RECIEPTS";
                public static readonly string SCHEDULAR_KEY_POST_PO_ORDERS = "POST_PO_ORDERS";
                public static readonly string DOMAIN_SETTINGS_PURCHASE_ORDER_MASTER_CONFIG = "PURCHASE_ORDER_MASTER_CONFIG";

                public static readonly string SCHEDULER_HOUR_ARIBA = "AribaSettings:SchedulerHour";
                public static readonly string SCHEDULER_MINUTE_ARIBA = "AribaSettings:SchedulerMinute";
                public static readonly string SCHEDULAR_KEY_FETCH_ARIBA_INVOICES = "FETCH_ARIBA_INVOICES";

                public static readonly string DOMAIN_SETTINGS_JOB_SCHEDULAR_CONFIG = "JOB_SCHEDULAR_CONFIG";
                public const string EXPIRY_DATE_PLACEHOLDER = "{{EXPIRY_DATE}}";
                public const string DAYS_LEFT_PLACEHOLDER = "{{DAYS_LEFT}}";
                public const string REMAINDER_EMAIL_KEY = "REMAINDER_EMAIL";
                public const string DOMAIN_ENTITY_TYPE = "DOMAIN";
                public const string REMAINDER_EMAIL_EXPIRED_KEY = "REMAINDER_EMAIL_EXPIRED";
                public const string REMAINDER_EMAIL_EXPIRING_KEY = "REMAINDER_EMAIL_EXPIRING";
                public const string STATUS_ARIBA_APPROVE = "approve";
                public const string STATUS_PENDING = "Pending";
                public static readonly string PR_ARIBA_SCHEDULAR_KEY = "PR_ARIBA_SCHEDULAR_KEY";
                public static readonly string DOMAIN_SETTINGS_ARIBA_TOKEN_INVOICE = "ARIBA_TOKEN_INVOICE";
                public static readonly Guid BUYER_USER_TYPE = Guid.Parse("d9cb966d-4dc3-40b0-b10a-2be022f11ebc");
                public static readonly string ARIBA_CONTRACT_JOB_KEY = "ARIBA_CONTRACT_JOB_KEY";
                public static readonly string ACTION_PERFORMED_BY_PLACEHOLDER = "{ACTION_PERFORMED_BY}";
                public static readonly string PYTHON_USER_DB = "PythonDBString:DefaultConnection";
                public static readonly string ARIBA_PORTAK_LINK = "Origin:HostOrigin";
                public const string INVOICE_ID_PLACEHOLDER = "{INVOICE_ID}";
                public static readonly string APPROVAL_LINK_PLACEHOLDER = "{APPROVAL_LINK}";
                public static readonly string ARIBA_APPROVAL_EMAIL_KEY = "APPROVAL_REQUEST";
                public const string CONTRACT_APPROVAL_EMAIL_KEY = "CONTRACT_APPROVAL_REQUEST";
                public const string REQUISITION_ID_PLACEHOLDER = "{REQUISITION_ID}";
                public const string REQUESTER_NAME_PLACEHOLDER = "{REQUESTER_NAME}";
                public const string CONTRACT_TASK_ID_PLACEHOLDER = "{CONTRACTTASKID}";
                public const string CONTRACT_BEGIN_DATE_PLACEHOLDER = "{CONTRACT_BEGIN_DATE}";
                public const string CONTRACT_TOTAL_AMOUNT_PLACEHOLDER = "{CONTRACT_TOTAL_AMOUNT}";
                public const string CONTRACT_CURRENCY_PLACEHOLDER = "{CONTRACT_CURRENCY}";
                public const string CONTRACT_AMOUNT_SPEND_PLACEHOLDER = "{CONTRACT_AMOUNT_SPEND}";
                public const string CONTRACT_BALANCE_AMOUNT_PLACEHOLDER = "{CONTRACT_BALANCE_AMOUNT}";

                public const string SUPPLIER_NAME_PLACEHOLDER = "{SUPPLIER_NAME}";
                public const string AMOUNT_PLACEHOLDER = "{AMOUNT}";
                public const string INVOICE_ENTITY = "ARIBA_APPROVAL";
                public const string DUE_DATE_PLACEHOLDER = "{DUE_DATE}";
                public static readonly string CLOSED_STATUS_ARIBA = "Closed";
                public static readonly string SYNC_DATA_CONFIG = "SYNC_DATA_CONFIG";
                public static readonly string ARIBA_APPROVED_STATUS_DISPLAY = "Approved";
                public static readonly string ARIBA_DENY_STATUS_DISPLAY = "Deny";
                public static readonly string STATUS_ARIBA_DENY = "deny";

                public static readonly string SCHEDULAR_KEY_FETCH_ARIBA_REQUISITIONS = "FETCH_ARIBA_REQUISITIONS";
                public static readonly string SCHEDULAR_KEY_FETCH_ARIBA_CONTRACT = "FETCH_ARIBA_CONTRACT";
                public static readonly string DOMAIN_SETTINGS_CONTRACT_URL = "CONTRACT_URL";
                public static readonly string DOMAIN_SETTINGS_ARIBA_CONTRACT_TOKEN = "ARIBA_CONTRACT_TOKEN";
                public const string TYPE_CONTRACT = "CONTRACT";
                public const string TYPE_INVOICE = "INVOICE";
                public const string TYPE_PR = "PR";
                public const string TYPE_PR_UPLOAD = "PR_UPLOAD";
                public static readonly string GATEKEEPER_APPROVAL_EMAIL_KEY = "GATEKEEPER_APPROVAL_NOTIFICATION";
                public const string TYPE_INVOICE_UPLOAD = "INVOICE_UPLOAD";
                public const string TYPE_CONTRACT_UPLOAD = "CONTRACT_UPLOAD";
                public const string TYPE_MATERIAL_PR_UPLOAD = "MATERIAL_PR_UPLOAD";
                public static readonly string ARIBA_PR_EMAIL_NOTIFICATION_KEY = "PR_APPROVAL_REQUEST";

                public const string PR_NUMBER_PLACEHOLDER = "{PR_NUMBER}";

                public const string STATUS_PLACEHOLDER = "{STATUS}";
                public static readonly string MATERIAL_PR_CREATION_EMAIL_KEY = "MATERIAL_PR_CREATION";
                public static readonly string MATERIAL_PR_UPDATE_EMAIL_KEY = "MATERIAL_PR_UPDATE";
                public static readonly string ARIBA_CONTRACT_EMAIL_NOTIFICATION_KEY = "CONTRACT_APPROVAL_REQUEST";
                public static readonly string MAX_REQUEST_SIZE = "MaxRequestBodySize";
                public static readonly string LOGIN_ATTRIBUTE_LOGIN = "LOGIN";
                public static readonly string SCHEDULAR_KEY_POST_MATERIAL_PR = "MaterialPR";
                public static readonly string DOMAIN_SETTINGS_MATERIAL_PR_POSTING = "MATERIAL_PR";
                public static readonly string MATERIAL_TYPE = "Material";
                public static readonly string ACCOUNT_TYPE = "EXPENSES";
                public static readonly string MATERIAL_PR_STATUS_COMPOSING = "Composing";

                public static readonly string MATERIAL_PR_STATUS_PENDING = "Pending";
                public static readonly string PYTHON_USER_DB_NAME = "PythonDBString:DatabaseName";
                public static readonly string TYPE_PAYMENT = "PAYMENT";
                public static readonly string PAID_STATUS = "PAID";
                public static readonly string PAYING_STATUS = "PAYING";
                public static readonly string TICKET_SYSTEM_DB_NAME = "ConnectionStrings:DatabaseName";
                public static readonly string SERVICE_ACCOUNT_TYPE = "EXPENSES";
                public static readonly string SERVICE_TYPE = "SERVICE";
                public static readonly string UAT_ENVIRONMENT = "UAT";
                public static readonly string DOMAIN_COOKIE_NAME = "Domain:DomainName";

                public static readonly string DEFAULT_CURRENCY = "AED";
                public static readonly string COMMODITY_CODES_WITH_TAX = "0112";
                public static readonly string COMMODITY_CODES_WITHOUT_TAX = "0512";

                public static readonly string INVOICE_TYPE_PRATTACHMENT = "INVOICE_PRATTACHMENT";
                public static readonly string MASTER_VENUE_TYPE = "VENUE";
                public static readonly string MASTER_SUPPLIER_TYPE = "SUPPLIER";
                public static readonly string DOMAIN_SETTINGS_AUTO_INVOICE_POSTING = "AUTO_INVOICE_POSTING";
                public static readonly string DRAFT_ID_START_WITH_PR = "PR";
                public static readonly string OPERATION_MATERIAL_PR_CREATION = "New";
                public static readonly string OPERATION_MATERIAL_PR_UPDATE = "Update";
                public static readonly string PR_ENTERTAINMENT_USER_EMAIL_ONE_KEY = "neha.khandagale@fivehotelsandresorts.com";
                public static readonly string PR_ENTERTAINMENT_USER_EMAIL_TWO_KEY = "daniel@fiveglobalholdings.com";
                public static readonly string PUBLIC_RELATION_COMMODITY_CODE_WITH_TAX = "0579";
                public static readonly string PUBLIC_RELATION_COMMODITY_CODE_WITHOUT_TAX = "0579";
                public static readonly string CONCIEAGE_COMMODITY_CODES_WITH_TAX = "0112";
                public static readonly string CONCIEAGE_COMMODITY_CODES_WITHOUT_TAX = "0512";
                public static readonly string OPERATION_SERVICE_PR_CREATION = "New";
                public static readonly string OPERATION_SERVICE_PR_UPDATE = "Update";
                public static readonly string TYPE_SERVICE_PR_UPLOAD = "SERVICE_PR_UPLOAD";
                public static readonly string SERVICE_PR_CREATION_EMAIL_KEY = "SERVICE_PR_CREATION";
                public static readonly string SERVICE_PR_UPDATE_EMAIL_KEY = "SERVICE_PR_UPDATE";
                public static readonly string PR_ENTERTAINMENT_MATERIAL_CODE = "1006929";
                public static readonly string PUBLIC_RELATION_MATERIAL_CODE = "1023900";
                public static readonly string CONCIERGE_MATERIAL_CODE = "1023960";
                public static readonly string SERVICE_PR_ENTERTAINMENT_MATERIAL_CODE = "1006929";
                public static readonly string SERVICE_PR_PUBLIC_RELATION_MATERIAL_CODE = "1006929";
                public static readonly string SERVICE_PR_CONCIERGE_MATERIAL_CODE = "1006929";
                public static readonly string DOMAIN_SETTINGS_AUTO_SES_POSTING = "AUTO_SES_POSTING";
                public static readonly string INVOICE_TYPE_PR_SERVICE_ATTACHMENT = "INVOICE_PR_SERVICE_ATTACHMENT";
                public static readonly string PUBLIC_RELATION_SERVICE_COMMODITY_CODE_TAX = "0579";
                public static readonly string CONCIEAGE_SERVICE_COMMODITY_CODES_WITH_TAX = "0553";

                public static readonly string PR_ENTERINMENT_SERVICE_COMMODITY_CODE_WITHOUT_TAX = "0512";
                public static readonly string LEGAL_DEPARTMENT_MATERIAL_COMMODITY_CODE_WITHOUT_TAX = "0513";
                public static readonly string LEGAL_DEPARTMENT_MATERIAL_COMMODITY_CODES_WITH_TAX = "0607";
                public static readonly string LEGAL_DEPARTMENT_MATERIAL_CODE = "1006929";
                public static readonly string PR_MATERIAL_TYPE = "MATERIAL";
                public static readonly string PR_SERVICE_TYPE = "SERVICE";
                public static readonly string MASTER_CONTRACT_DEPARTMENT_TYPE = "DEPARTMENT";
                public static readonly string MASTER_CONTRACT_REGION_TYPE = "REGION";
                public static readonly string MASTER_CONTRACT_COMODITY_TYPE = "COMODITY";
                public static readonly string MASTER_CONTRACT_TYPE = "ContractType";
                public static readonly string MASTER_CONTRACT_HIERARCHICAL_TYPE = "HierarchicalType";
                public static readonly string MASTER_CONTRACT_TERM_TYPE = "ExpirationTermType";
                public static readonly string DOMAIN_SETTINGS_EASY_CONTRACT_CONFIG = "EASY_CONTRACT_CONFIG";
                public static readonly string LEGAL_DEPARTMENT_UOM = "LH";
                public static readonly int PR_TITLE_LIMIT = 120;
                public static readonly string PR_MATERIAL_ITEM_CATEGORY = "M";
                public static readonly string PR_SERVICE_ITEM_CATEGORY = "D";
                public static readonly string PR_ENTERTAINMENT_DEPARTMENT = "Entertainment";
                public static readonly string EASY_CONTRACT_HIERARCHICAL_TYPE = "StandAlone";
                public static readonly string MORETHAN_8_HOURS_BUCKET = "MORETHAN_8_HOURS";




        }






}