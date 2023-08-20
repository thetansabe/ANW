export class CharUtils{

    private static STR="qwertyuiopasdfghjklzxcvbnmQWERTYUIUOPASDFGHJKLZXCVBNM1234567890!@#$%^&";

    static generateRandomString(length: number){
        let str="";
        while (str.length<length)
        {
            const index=Math.floor(Math.random() * (CharUtils.STR.length-1));
            str+= CharUtils.STR.charAt(index);
        }
        return str;
    }
}