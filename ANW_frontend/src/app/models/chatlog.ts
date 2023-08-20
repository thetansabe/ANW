export class ChatLog{
    UserId: string;
    Content: ChatContent[];
    UpdatedOn: Date;
    Message?: string;
}
export class ChatContent{
    Content: string;
    CreatedOn: Date;
    NotSend?: boolean;
    Error?:boolean;
}