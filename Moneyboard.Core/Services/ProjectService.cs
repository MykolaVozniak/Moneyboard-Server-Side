﻿using AutoMapper;
using AutoMapper.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moneyboard.Core.DTO.ProjectDTO;
using Moneyboard.Core.DTO.UserDTO;
using Moneyboard.Core.Entities.BankCardEntity;
using Moneyboard.Core.Entities.ProjectEntity;
using Moneyboard.Core.Entities.RoleEntity;
using Moneyboard.Core.Entities.UserEntity;
using Moneyboard.Core.Entities.UserProjectEntity;
using Moneyboard.Core.Exeptions;
using Moneyboard.Core.Interfaces.Repository;
using Moneyboard.Core.Interfaces.Services;
using Moneyboard.Core.Resources;
using System.Data;

namespace Moneyboard.Core.Services
{
    public class ProjectService : IProjectService
    {
        protected readonly IMapper _mapper;
        protected readonly UserManager<User> _userManager;
        protected IRepository<Project> _projectRepository;
        protected IRepository<BankCard> _bankCardRepository;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly IRoleService _roleService;
        protected readonly IRepository<UserProject> _userProjectRepository;
        protected readonly IRepository<User> _userRepository;
        protected readonly IRepository<Role> _roleRepository;

        public ProjectService(
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IRepository<Project> projectRepository,
            IRepository<BankCard> bankCardBaseRepository,
            IRoleService roleService,
            IRepository<User> userRepository,
            IRepository<Role> roleRepository,
            IRepository<UserProject> userProjectRepository,
            UserManager<User> userManager)
        {
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _projectRepository = projectRepository;
            _bankCardRepository = bankCardBaseRepository;
            _roleService = roleService;
            _userProjectRepository = userProjectRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _roleRepository = roleRepository;
        }


        public async Task<ProjectIdDTO> CreateNewProjectAsync(ProjectCreateDTO projectDTO, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new HttpException(System.Net.HttpStatusCode.BadRequest, ErrorMessages.UserNotFound);

            var existingProjects = await _projectRepository.GetListAsync(
                p => p.Name == projectDTO.Name && p.UserProjects.Any(up => up.UserId == userId));

            if (existingProjects.Any())
                throw new HttpException(System.Net.HttpStatusCode.BadRequest, "Project with the same name already exists for this user.");

            var bankcard = _mapper.Map<BankCard>(projectDTO);
            var project = _mapper.Map<Project>(projectDTO);
            project.ProjectPoinPercent = 0;
            project.CreateDate = DateTime.Now;
            project.SalaryDate = GetSalaryDate(projectDTO.SalaryDay);

            if (await _bankCardRepository.GetEntityAsync(x => x.CardNumber == projectDTO.CardNumber) == null)
            {
                project.BankCard = bankcard;
                await _bankCardRepository.AddAsync(bankcard);
                await _bankCardRepository.SaveChangesAsync();
            }
            else
            {
                project.BankCard = await _bankCardRepository.GetEntityAsync(x => x.CardNumber == projectDTO.CardNumber);
                await _bankCardRepository.SaveChangesAsync();
            }

            await _projectRepository.AddAsync(project);
            await _projectRepository.SaveChangesAsync();
            await CreateNewRole(project.ProjectId, userId);

            return new ProjectIdDTO { ProjectId = project.ProjectId };
        }

        private async Task CreateNewRole(int projectId, string userId)
        {
            var project = await _projectRepository.GetByKeyAsync(projectId);
            if (project == null)
                throw new HttpException(System.Net.HttpStatusCode.BadRequest, ErrorMessages.ProjectNotFound);

            Role Owner;
            Owner = new Role
            {
                RoleName = "Owner",
                RolePoints = 0,
                CreateDate = DateTime.Now.Date,
                ProjectId = projectId,
                IsDefolt = true,
            };

            Role Member;
            Member = new Role
            {
                RoleName = "Member",
                RolePoints = 0,
                CreateDate = DateTime.Now.Date,
                ProjectId = projectId,
                IsDefolt = false,
            };

            await _roleRepository.AddAsync(Owner);
            await _roleRepository.AddAsync(Member);          
            await _roleRepository.SaveChangesAsync();
            await CreateUserProject(userId, projectId, true, Owner);
        }
        private DateTime GetSalaryDate(int salaryDay)
        {
            DateTime today = DateTime.Today;

            if (today.Day >= salaryDay)
            {
                today = today.AddMonths(1);
            }

            DateTime targetDate = new DateTime(today.Year, today.Month, salaryDay);

            return targetDate;
        }
        public async Task AddMemberToProjectAsync(string userId, int projectId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new HttpException(System.Net.HttpStatusCode.BadRequest, ErrorMessages.UserNotFound);

            var existingUserProject = await _userProjectRepository.GetListAsync(up => up.UserId == userId && up.ProjectId == projectId);
            if (existingUserProject.Any())
                throw new HttpException(System.Net.HttpStatusCode.BadRequest, "User is already a member of this project.");

            var role = await _roleRepository.GetEntityAsync(x => x.ProjectId == projectId && x.IsDefolt == false);
            if (role == null)
                throw new HttpException(System.Net.HttpStatusCode.BadRequest, "Role not found.");

            await CreateUserProject(userId, projectId, false, role);

        }

        private async Task CreateUserProject(string userId, int projectId, bool isOwner, Role role)
        {
            var project = await _projectRepository.GetByKeyAsync(projectId);

            if (project == null)
                throw new HttpException(System.Net.HttpStatusCode.BadRequest, ErrorMessages.ProjectNotFound);

            var userProject = new UserProject
            {
                IsOwner = isOwner,
                MemberDate = DateTime.Now.Date,
                PersonalPoints = 0,
                UserId = userId,
                Project = project,
                Role = role
            };

            await _userProjectRepository.AddAsync(userProject);
            await _userProjectRepository.SaveChangesAsync();
        }

        public async Task<ProjectInfoDTO> InfoFromProjectAsync(int projectId, string userId)
        {
            var project = await _projectRepository.GetByKeyAsync(projectId);
            if (project == null)
                throw new HttpException(System.Net.HttpStatusCode.BadRequest, ErrorMessages.ProjectNotFound);

            var userProject = await _userProjectRepository.GetEntityAsync(x => x.UserId == userId && x.ProjectId == projectId);
            bool? isOwner;
            if (userProject == null)
                isOwner = null;
            else
                isOwner = userProject != null && userProject.IsOwner == true;

            var projectInfo = _mapper.Map<ProjectInfoDTO>(project);
            projectInfo.IsOwner = isOwner;

            var userProjectOwer = await _userProjectRepository.GetEntityAsync(x => x.ProjectId == projectId && x.IsOwner == true);
            var userOwner = await _userRepository.GetByKeyAsync(userProjectOwer.UserId);
            projectInfo.OwnerName = userOwner.Firstname + " " + userOwner.Lastname;
            projectInfo.OwnerURL = userOwner.ImageUrl;
            return projectInfo;
        }

        public async Task EditProjectDateAsync(ProjectEditDTO projectEditDTO, int projectId, string userId)
        {
            var project = await _projectRepository.GetByKeyAsync(projectId);

            if (project == null)
                throw new HttpException(System.Net.HttpStatusCode.BadRequest, ErrorMessages.ProjectNotFound);


            project = _mapper.Map(projectEditDTO, project);


            await _projectRepository.UpdateAsync(project);
            await _projectRepository.SaveChangesAsync();

        }
        public async Task EditProjectPointPrecent(ProjectPointDTO projectPointProcent, int projectId)
        {
            var project = await _projectRepository.GetByKeyAsync(projectId);
            if (project == null)
                throw new HttpException(System.Net.HttpStatusCode.BadRequest, ErrorMessages.ProjectNotFound);

            project.ProjectPoinPercent = projectPointProcent.ProjectPoinPercent;

            await _projectRepository.UpdateAsync(project);
            await _projectRepository.SaveChangesAsync();
        }
        public async Task<IEnumerable<ProjectForUserDTO>> GetProjectsOwnedByUserAsync(string userId)
        {
            var userProjects = await _userProjectRepository.GetListAsync(up => up.UserId == userId && up.IsOwner);

            var projectIds = userProjects.Select(up => up.ProjectId);

            var projects = await _projectRepository.GetListAsync(p => projectIds.Contains(p.ProjectId));

            var projectDtos = _mapper.Map<IEnumerable<ProjectForUserDTO>>(projects);

            return projectDtos;
        }

        public async Task<IEnumerable<ProjectForUserDTO>> GetProjectsUserIsMemberAsync(string userId)
        {
            var userProjects = await _userProjectRepository.GetListAsync(up => up.UserId == userId && !up.IsOwner);

            var projectIds = userProjects.Select(up => up.ProjectId);

            var projects = await _projectRepository.GetListAsync(p => projectIds.Contains(p.ProjectId));

            var projectDtos = _mapper.Map<IEnumerable<ProjectForUserDTO>>(projects);

            return projectDtos;
        }
        public async Task<ProjectDetailsDTO> GetProjectDetailsAsync(int projectId, string userId)
        {
            var userProjectTest = await _userProjectRepository.GetEntityAsync(x => x.UserId == userId && x.ProjectId == projectId);
            if (userProjectTest == null || userProjectTest.IsOwner == null)
                throw new HttpException(System.Net.HttpStatusCode.BadRequest, ErrorMessages.ProjectNotFound);

            var project = await _projectRepository.GetByKeyAsync(projectId);
            if (project == null)
                throw new HttpException(System.Net.HttpStatusCode.BadRequest, ErrorMessages.ProjectNotFound);

            var projectDTO = _mapper.Map<ProjectDetailsDTO>(project);

            var userProjects = await _userProjectRepository.GetListAsync(x => x.ProjectId == projectId);
            var userProjectUserIds = userProjects.Select(up => up.UserId).ToList();
            var roles = await _roleRepository.GetListAsync(x => x.ProjectId == projectId);
            var users = await _userRepository.GetListAsync(x => userProjectUserIds.Contains(x.Id));


            var memberDTOs = _mapper.Map<List<ProjectMemberDTO>>(userProjects);

            var usersDTO = _mapper.Map<List<ProjectMemberDTO>>(users);
            var rolesDTO = _mapper.Map<List<ProjectMemberDTO>>(roles);

            for (var i = 0; i < memberDTOs.Count; i++)
            {
                // Знаходимо відповідність між об'єктами за Id
                var userDTO = usersDTO.FirstOrDefault(u => u.UserId == userProjects.ElementAt(i).UserId);
                var roleDTO = rolesDTO.FirstOrDefault(r => r.RoleId == userProjects.ElementAt(i).RoleId);

                // Просто присвоюємо значення з мапованих об'єктів
                memberDTOs[i].UserName = userDTO.UserName;
                memberDTOs[i].ImageUrl = userDTO.ImageUrl;
                memberDTOs[i].RoleName = roleDTO.RoleName;
                memberDTOs[i].RolePoints = roleDTO.RolePoints;
                memberDTOs[i].IsDefolt = roleDTO.IsDefolt;
            }


            projectDTO.Members = memberDTOs;

            return projectDTO;
        }
        public async Task DeleteProjectAsync(int projectId, string userId)
        {
            var project = await _projectRepository.GetByKeyAsync(projectId);
            if (project == null)
                throw new HttpException(System.Net.HttpStatusCode.BadRequest, ErrorMessages.ProjectNotFound);

            var userProject = await _userProjectRepository.GetEntityAsync(x => x.UserId == userId && x.ProjectId == projectId);
            if (userProject == null || userProject.IsOwner != true)
                throw new HttpException(System.Net.HttpStatusCode.BadRequest, "Not enough rights");

            await _projectRepository.DeleteAsync(project);
            await _projectRepository.SaveChangesAsync();
        }
        public async Task LeaveTheProjectAsync(int projectId, string userId)
        {

            var project = await _projectRepository.GetByKeyAsync(projectId);
            if (project == null)
                throw new HttpException(System.Net.HttpStatusCode.BadRequest, ErrorMessages.ProjectNotFound);
            var userProject = await _userProjectRepository.GetEntityAsync(x => x.UserId == userId && x.ProjectId == projectId);

            if (userProject == null || userProject.IsOwner == true)
                throw new HttpException(System.Net.HttpStatusCode.BadRequest, "Impossible action");


            await _userProjectRepository.DeleteAsync(userProject);
            await _userProjectRepository.SaveChangesAsync();

        }

        public async Task EditProjectPointAsync(int projectId, ProjectPointDTO projectPointDTO)
        {
            var project = await _projectRepository.GetByKeyAsync(projectId);

            if (project == null)
                throw new HttpException(System.Net.HttpStatusCode.BadRequest, ErrorMessages.ProjectNotFound);

            project.ProjectPoinPercent = projectPointDTO.ProjectPoinPercent;

            await _projectRepository.UpdateAsync(project);
            await _projectRepository.SaveChangesAsync();

        }

        public async Task<double> CalculateTotalPayments(int projectId)
        {
            var project = await _projectRepository.GetByKeyAsync(projectId);
            if (project == null)
                throw new HttpException(System.Net.HttpStatusCode.BadRequest, ErrorMessages.ProjectNotFound);

            var projectMembers = await _userProjectRepository.GetListAsync(x => x.ProjectId == projectId);
  
            double totalPayments = projectMembers.Count()*project.BaseSalary;
            var bankCard = await _bankCardRepository.GetByKeyAsync(project.BankCardId);

            List <UserCalculatorPaymentDTO > memberList = new List<UserCalculatorPaymentDTO>();
            int allPoint = 0;
            foreach(var member in projectMembers)
            {
                var projectPaymentDTO = _mapper.Map<UserCalculatorPaymentDTO>(await _roleRepository.GetByKeyAsync(member.RoleId));

                projectPaymentDTO.ProjectPoinPercent = project.ProjectPoinPercent;
                projectPaymentDTO.PersonalPoints = member.PersonalPoints;
                memberList.Add(projectPaymentDTO);
                allPoint += projectPaymentDTO.RolePoints + projectPaymentDTO.PersonalPoints;
            }

            for(int i = 0; i < projectMembers.Count(); i++)
            {
                double memberPayment = CalculateMemberPayment(memberList[i], totalPayments - bankCard.Money, allPoint);
                totalPayments += memberPayment;
            }
            
            if(totalPayments>bankCard.Money)
                throw new HttpException(System.Net.HttpStatusCode.BadRequest, "Not many");

            return totalPayments;
        }

        private double CalculateMemberPayment(UserCalculatorPaymentDTO member, double money, int allPoint)
        {
            double procentMoney = member.ProjectPoinPercent *money / 100;
            if (allPoint == 0)
                return 0;

            double paymentPoint = procentMoney * money * member.RolePoints / allPoint + procentMoney * member.PersonalPoints / allPoint;

            return paymentPoint;
        }
    }
}
