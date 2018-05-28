using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uniconta.API.DebtorCreditor;
using Uniconta.API.Service;
using Uniconta.API.System;
using Uniconta.Common;
using Uniconta.DataModel;

namespace UnicontaBasics.Core.Managers
{

    public static class UnicontaAPIManager
    {
        private static string _username;
        private static string _password;
        private static CrudAPI crudAPI;
        private static UnicontaConnection _connection;
        private static Session _session;
        private static Guid _key;
        private static Company currentCompany;
        private static Company[] companies;

        public static void Initialize()
        {
            if (_session != null)
                return;

            _key = new Guid("00000000 -0000-0000-0000-000000000000");
            _connection = new UnicontaConnection(APITarget.Live);
            _session = new Session(_connection);
        }
        
        public static User getUser()
        {
            return _session.User;
        }

        #region login logout methods
        public async static Task<ErrorCodes> Login(string username, string password) {
            if (_session == null)
                throw new InvalidOperationException("API manager not initialized");

            if(_key.Equals(Guid.Empty))
                throw new InvalidOperationException("API key not set");

            if (_username == null || _password == null)
                _username = username;
                _password = password;

            var result = await _session.LoginAsync(_username, _password, Uniconta.Common.User.LoginType.API, _key);
            return result;
        }

        public static void Logout() {
            _session.LogOut();
        }
        #endregion


        #region Company methods

        public static async Task InitializeCompanies()
        {
            // Check if logged in
            if (await _session.IsLoggedIn() != ErrorCodes.Succes)
                throw new InvalidOperationException();

            // Getting companies
            companies = await _session.GetCompanies();

            // Getting user default company
            if (_session.User._DefaultCompany != 0)
                await SetCurrentCompany(

                    _session.User._DefaultCompany
                    
                    );
            else
            {
                var company = companies.FirstOrDefault();
                if (company == null)
                    throw new InvalidOperationException();

                await SetCurrentCompany(company);
            }
        }

        public static int GetCurrentCompanyId() { return currentCompany.CompanyId; }
        public static Company GetCurrentCompany() { return currentCompany; }

        public async static Task SetCurrentCompany(Company company) { await SetCurrentCompany(company.CompanyId); }
        public async static Task SetCurrentCompany(int companyId)
        {
            if (_session == null)
                throw new InvalidOperationException();

            currentCompany = await _session.OpenCompany(companyId, true);
        }

        #endregion

        #region API methods
        public static CrudAPI GetCrudAPI(Company company = null)
        {
            return new CrudAPI(_session, company);
        }

        public static InvoiceAPI GetInvoiceAPI(Company company = null)
        {
            return new InvoiceAPI(_session, company);
        }
        #endregion
    }
}
