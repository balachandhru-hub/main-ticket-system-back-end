using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dto
{
    public class ContractDetailDto
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? DisplayStatus { get; set; }
        public string? WorkspaceId { get; set; }
        public string? WorkspaceTitle { get; set; }
        public long BeginDate { get; set; }
        public List<Comment>? Comments { get; set; }
        public List<ApprovalRequest>? ApprovalRequests { get; set; }
         public AribaWorkspaceResponseDto WorkspaceDetails { get; set; } 
       public AribaDocument? Document { get; set; }
       public AribaDocumentContent? WorkSpace {get;set;}
    }

  
    public class AribaDocument
    {
        public string? Id { get; set; }
        public string? AribaType { get; set; }
        public string? EntityType { get; set; }
          
        public AribaDocumentContent? Content { get; set; }
    }

    // ================= FOLDER CONTENT =================

    public class AribaDocumentContent
    {
      
        public string? Id { get; set; }

        public string? WorkspaceId { get; set; }

   
        public string? Title { get; set; }

    
        public List<AribaSubContentDocument>? SubContentDoc { get; set; }
    }


    public class AribaSubContentDocument
    {
        
        public string? Id { get; set; }

        public string? AribaType { get; set; }

      
        public AribaSubContentDetail? Content { get; set; }
    }

    public class AribaSubContentDetail
    {
       
        public string? Id { get; set; }


        public string? Title { get; set; }


        public string? DocAttachmentId { get; set; }

 
        public string? ContentType { get; set; }


        public string? Extension { get; set; }

    
        public long Size { get; set; }

    
        public bool File { get; set; }

     
        public bool Deleted { get; set; }
    }
    public class Comment
    {
        public string? Id { get; set; }
        public long Date { get; set; }
        public string? Body { get; set; }
        public CommentUser? User { get; set; }
        public string? TaskId { get; set; }
        public string? State { get; set; }
        public string? Round { get; set; }
    }

    public class CommentUser
    {
        public string? PasswordAdapter { get; set; }
        public string? UniqueName { get; set; }
        public string? Name { get; set; }
        public string? EmailAddress { get; set; }
        public string? Phone { get; set; }
        public string? Realm { get; set; }
    }

    public class ApprovalRequest
    {
        public string? Id { get; set; }
        public string? TaskId { get; set; }
        public int State { get; set; }
        public string? ApprovalState { get; set; }
        public string? ApprovalStateLocalized { get; set; }
        public string? Reason { get; set; }
        public bool ApprovalRequired { get; set; }
        public List<string>? DependencyApprovalRequestsUID { get; set; }
        public List<Approver>? Approvers { get; set; }
        public long ActivationDate { get; set; }
    }

    public class Approver
    {
        public ApproverUser? User { get; set; }
        public ApproverGroup? Group { get; set; }
        public string? Type { get; set; }
    }

    public class ApproverUser
    {
        public string? PasswordAdapter { get; set; }
        public string? UniqueName { get; set; }
        public string? Name { get; set; }
        public string? EmailAddress { get; set; }
        public string? Phone { get; set; }
        public string? Realm { get; set; }
    }

    public class ApproverGroup
    {
        public string? UniqueName { get; set; }
        public string? Id { get; set; }
        public string? Name { get; set; }
    }

   
   
}
