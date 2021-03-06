﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MembershipHandler.Views
{
    public class CommitteeList
    {
        public List<CommitteeSingle> Members { get; set; }

        public CommitteeList()
        {
            Members = new List<CommitteeSingle>();
        }
    }

    public class CommitteeSingle
    {
        public string Title { get; set; }
        public string Name { get; set; }
    }

    public static class CommitteeListHelpers
    {
        public static CommitteeList ToCommitteeList(this List<Models.Committee> committee, Controllers.BaseFacebookController controller)
        {
            CommitteeList results = new CommitteeList();
            for (int i = 0; i < committee.Count; i++)
            {
                Models.Member member = controller.GetMember(committee[i].RowKey);
                results.Members.Add(new CommitteeSingle() { Name = member.Name, Title = committee[i].Title });
            }
            return results;
        }

        public static string Serialise(this CommitteeList committeeList)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(committeeList);
        }

        public static CommitteeList DeserialiseAsCommitteeList(this string serialisedString)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<CommitteeList>(serialisedString);
        }
    }
}