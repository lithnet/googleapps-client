using System;
using System.Collections.Generic;
using System.Linq;
using G = Google.Apis.Admin.Directory.directory_v1.Data;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public class GroupMembership
    {
        private static List<string> internalDomains;

        private static List<string> InternalDomains => GroupMembership.internalDomains;

        public HashSet<string> Members { get; set; }

        public HashSet<string> ExternalMembers { get; set; }

        public HashSet<string> Managers { get; set; }

        public HashSet<string> ExternalManagers { get; set; }

        public HashSet<string> Owners { get; set; }

        public HashSet<string> ExternalOwners { get; set; }

        public int Count
        {
            get
            {
                return this.Members.Count +
                    this.ExternalMembers.Count + 
                    this.Managers.Count + 
                    this.ExternalManagers.Count + 
                    this.Owners.Count + 
                    this.ExternalOwners.Count;
            }
        }

        public GroupMembership()
        {
            this.Members = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
            this.ExternalMembers = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
            this.Managers = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
            this.ExternalManagers = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
            this.Owners = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
            this.ExternalOwners = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
        }

        public HashSet<string> GetAllMembers()
        {
            HashSet<string> consolidatedMembers = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

            foreach (string member in this.Members)
            {
                consolidatedMembers.Add(member);
            }

            foreach (string member in this.ExternalMembers)
            {
                consolidatedMembers.Add(member);
            }

            foreach (string member in this.Managers)
            {
                consolidatedMembers.Add(member);
            }

            foreach (string member in this.ExternalManagers)
            {
                consolidatedMembers.Add(member);
            }

            foreach (string member in this.Owners)
            {
                consolidatedMembers.Add(member);
            }

            foreach (string member in this.ExternalOwners)
            {
                consolidatedMembers.Add(member);
            }

            return consolidatedMembers;
        }

        public List<G.Member> ToMemberList()
        {
            List<G.Member> members = new List<G.Member>();

            foreach (string address in this.Members.Union(this.ExternalMembers))
            {
                G.Member member = new G.Member
                {
                    Role = "MEMBER",
                    Email = address
                };

                members.Add(member);
            }

            foreach (string address in this.Managers.Union(this.ExternalManagers))
            {
                G.Member member = new G.Member
                {
                    Role = "MANAGER",
                    Email = address
                };

                members.Add(member);
            }

            foreach (string address in this.Owners.Union(this.ExternalOwners))
            {
                G.Member member = new G.Member
                {
                    Role = "OWNER",
                    Email = address
                };

                members.Add(member);
            }

            return members;
        }

        public static void GetInternalDomains(string customerID)
        {
            GroupMembership.internalDomains = new List<string>();

            foreach (Domain domain in DomainsRequestFactory.List(customerID).Domains)
            {
                GroupMembership.internalDomains.Add(domain.DomainName);

                if (domain.DomainAliases != null)
                {
                    foreach (DomainAlias alias in domain.DomainAliases)
                    {
                        GroupMembership.internalDomains.Add(alias.DomainAliasName);
                    }
                }
            }
        }

        public void AddMember(G.Member member)
        {
            if (string.IsNullOrWhiteSpace(member.Email))
            {
                return;
            }

            switch (member.Role.ToLowerInvariant())
            {
                case "member":
                    this.AddMember(member.Email);
                    break;

                case "manager":
                    this.AddManager(member.Email);
                    break;

                case "owner":
                    this.AddOwner(member.Email);
                    break;

                default:
                    throw new NotSupportedException("Unknown or unsupported member role");
            }
        }

        public void AddMember(string address)
        {
            if (this.IsAddressInternal(address))
            {
                this.Members.Add(address);
            }
            else
            {
                this.ExternalMembers.Add(address);
            }
        }

        public void AddOwner(string address)
        {
            if(this.IsAddressInternal(address))
            {
                this.Owners.Add(address);
            }
            else
            {
                this.ExternalOwners.Add(address);
            }
        }

        private void AddManager(string address)
        {
            if (this.IsAddressInternal(address))
            {
                this.Managers.Add(address);
            }
            else
            {
                this.ExternalManagers.Add(address);
            }
        }

        private bool IsAddressInternal(string address)
        {
            string[] split = address.Split('@');

            if (split.Length != 2)
            {
                throw new ArgumentException("The specified address was not a valid email address: " + address, nameof(address));
            }

            if (GroupMembership.InternalDomains == null || GroupMembership.InternalDomains.Count == 0)
            {
                return true;
            }

            if (GroupMembership.InternalDomains.Contains(split[1], StringComparer.CurrentCultureIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}