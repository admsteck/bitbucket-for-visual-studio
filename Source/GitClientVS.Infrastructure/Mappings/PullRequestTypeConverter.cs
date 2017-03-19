﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using BitBucket.REST.API.Helpers;
using BitBucket.REST.API.Models;
using BitBucket.REST.API.Models.Standard;
using GitClientVS.Contracts.Models.GitClientModels;
using GitClientVS.Infrastructure.Extensions;
using GitClientVS.Infrastructure.Utils;

namespace GitClientVS.Infrastructure.Mappings
{
    public class PullRequestTypeConverter
    : ITypeConverter<PullRequest, GitPullRequest>
    {
        public GitPullRequest Convert(PullRequest source, GitPullRequest destination, ResolutionContext context)
        {
            Dictionary<GitUser, bool> reviewers = new Dictionary<GitUser, bool>();
            if (source.Reviewers != null)
            {
                foreach (var reviewer in source.Reviewers)
                {
                    reviewers.Add(reviewer.MapTo<GitUser>(), ApiHelpers.GetApproveStatus(reviewer.Username, source.Participants));
                }
            }

            return new GitPullRequest(source.Title, source.Description, source.Source.Branch.Name, source.Destination.Branch.Name)
            {
                Id = source.Id,
                Author = source.Author.MapTo<GitUser>(),
                Status = source.State.MapTo<GitPullRequestStatus>(),
                Created = TimeConverter.GetDate(source.CreatedOn),
                Updated = TimeConverter.GetDate(source.UpdatedOn),
                Link = source.Links.Html.Href,
                CloseSourceBranch = source.CloseSourceBranch,
                Url = source.Links.Html.Href,
                Reviewers = reviewers,
                CommentsCount = source.CommentsCount,
                Version = source.Version
            };
        }
    }

    public class ReversePullRequestTypeConverter
  : ITypeConverter<GitPullRequest, PullRequest>
    {
        public PullRequest Convert(GitPullRequest source, PullRequest destination, ResolutionContext context)
        {
            return new PullRequest()
            {
                Id = source.Id,
                Title = source.Title,
                Description = source.Description,
                Source = new Source
                {
                    Branch = new Branch()
                    {
                        Name = source.SourceBranch
                    },
                },
                Destination = new Source()
                {
                    Branch = new Branch()
                    {
                        Name = source.DestinationBranch
                    }
                },
                State = source.Status.MapTo<PullRequestOptions>(),
                CloseSourceBranch = source.CloseSourceBranch,
                Reviewers = source.Reviewers?.Select(x => new User() { Username = x.Key.Username, Type = "user" }).ToList(),
                Version = source.Version
            };
        }
    }
}