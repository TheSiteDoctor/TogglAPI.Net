using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Toggl.Interfaces;
using Toggl.QueryObjects;

namespace Toggl.Services
{
    public class ProjectService : IProjectService
    {
        private readonly string ProjectsUrl = ApiRoutes.Project.ProjectsUrl;


        private IApiService ToggleSrv { get; set; }


        public ProjectService(string apiKey)
            : this(new ApiService(apiKey))
        {

        }

        public ProjectService(IApiService srv)
        {
            ToggleSrv = srv;
        }

        /// <summary>
        /// 
        /// https://www.toggl.com/public/api#get_projects
        /// </summary>
        /// <returns></returns>
        public List<Project> List()
        {
            var lstProj = new List<Project>();
            var lstWrkSpc = ToggleSrv.Get(ApiRoutes.Workspace.ListWorkspaceUrl).GetData<List<Workspace>>();
            lstWrkSpc.ForEach(e =>
                {
                    var projs = ForWorkspace(e.Id.Value);
                    lstProj.AddRange(projs);
                });
            return lstProj;
        }

        public List<Project> ForWorkspace(int id)
        {
            var url = string.Format(ApiRoutes.Workspace.ListWorkspaceProjectsUrl, id);
            return ToggleSrv.Get(url).GetData<List<Project>>();
        }

        public List<Project> ForClient(Client client, ItemStatus status = ItemStatus.Active)
        {
            if (!client.Id.HasValue)
                throw new InvalidOperationException("Client Id not set");

            return ForClient(client.Id.Value, status);
        }

        public List<Project> ForClient(int id, ItemStatus status = ItemStatus.Active)
        {
            var url = string.Format(ApiRoutes.Client.ClientProjectsUrl, id);
            var active = status == ItemStatus.Both ? "both" : status == ItemStatus.Active ? "true" : "false";
            return ToggleSrv.Get(url, new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("active", active) }).GetData<List<Project>>();
        }

        public Project Get(int id)
        {
            return List().FirstOrDefault(w => w.Id == id);
        }

        public Project Add(Project project)
        {

            return ToggleSrv.Post(ProjectsUrl, project.ToJson()).GetData<Project>();
        }

        public bool Delete(int id)
        {
            var url = string.Format(ApiRoutes.Project.DetailUrl, id);
            var rsp = ToggleSrv.Delete(url);

            return rsp.StatusCode == HttpStatusCode.OK;
        }

        public bool DeleteIfAny(int[] ids)
        {
            if (!ids.Any() || ids == null)
                return true;
            return Delete(ids);
        }

        public bool Delete(int[] ids)
        {
            if (!ids.Any() || ids == null)
                throw new ArgumentNullException("ids");

            var url = string.Format(
                ApiRoutes.Project.ProjectsBulkDeleteUrl,
                string.Join(",", ids.Select(id => id.ToString()).ToArray()));

            var rsp = ToggleSrv.Delete(url);

            return rsp.StatusCode == HttpStatusCode.OK;
        }

    }
}
