﻿module FSheila.Smc

open ScanRat
open FSheila
open Parser
open System.Collections.Generic

let X = new Stack<string>()
let S = new Stack<Cmd>()
let M = new Dictionary<string, Cmd>()
let C = new Stack<Cmd>()


//Ta bugado. 3<>4 and 2<2 esse caso buga pq mistura number com bool na pilha S
let calculatorBool (X: Stack<string>) (S: Stack<Cmd>) :bool = 
    while X.Count <> 0 do
        let op = X.Pop()
        let d1 = S.Pop()
        //match op with
        //| "Neg" -> match d1 with 
        //           | Boolean a -> not a under construction
        let d2 = S.Pop()
        match (d1,d2) with
        | Boolean d1, Boolean d2 ->
            match op with
            | "And" -> S.Push(Boolean(d1 && d2))
            | "Or" -> S.Push(Boolean(d1 || d2))
        | Number n1, Number n2 -> //Operacoes bool com numeros
            match op with
            | "Eq" -> S.Push(Boolean(n1 = n2))
            | "Neq" -> S.Push(Boolean(n1 <> n2))
            | "Leb" -> S.Push(Boolean(n1 < n2))
            | "Leq" -> S.Push(Boolean(n1 <= n2))
            | "Geb" -> S.Push(Boolean(n1 > n2))
            | "Geq" -> S.Push(Boolean(n1 >= n2))

    match S.Pop() with
    | Boolean a -> a
    | Id a -> match M.Item(a) with //pega o valor na memória. Pela construção do parser, por mais que o F# reclame que não abrange todos os tipos que Cmd pode assumir, aqui só é esperado sempre um boolean.
              | Boolean x -> x

//let rec solveSimpleCalcExp (op:string) (d1: Cmd) (d2: Cmd) =   ideia pra uma caculadora unfiicada que possa ser reutilizada.
//    match (d1,d2) with
//    | Number a, Number b ->
//        match op with
//        | "Add"  -> (Number (a+b))
//        | "Subtract" -> (Number( a-b))
//        | "Multiply" -> (Number( a*b))
//        | "Divide" -> (Number(int(a/b)))
    

let calculator (X: Stack<string>) (S: Stack<Cmd>) :int = 
    while X.Count <> 0 do
        let op = X.Pop()
        let d1 = S.Pop()
        let d2 = S.Pop()
        match (d1,d2) with
        | Number d1, Number d2 ->
            match op with
            | "Add" -> S.Push(Number(d1 + d2))
            | "Subtract" -> S.Push(Number(d1 - d2))
            | "Multiply" -> S.Push(Number(d1 * d2))
            | "Divide" -> S.Push(Number(int(d1 / d2)))
    match S.Pop() with
    | Number a ->
        a
    | Id a -> match M.Item(a) with
              | Number a -> a
 
 //Faltando <>
let rec stackator (exp) =
    match exp with
    //essas três primeiras representam o "caso base" de algumas regras.
    | Number a -> S.Push( Number a)
    | Boolean b -> S.Push (Boolean b)
    | Id x -> S.Push (Id x)
    | Add (a, b) -> X.Push("Add"); match (a,b) with
                                    | Number x, Number y -> S.Push(Number x); S.Push(Number y)
                                    | Number x , d -> S.Push(Number x); stackator d; 
                                    | d , Number y -> S.Push (Number y); stackator d
                                    | v , k -> stackator (v); stackator (k)
    | Subtract (a, b) -> X.Push("Subtract"); match (a,b) with
                                   | Number x, Number y -> S.Push(Number y); S.Push(Number x); //nota: tanto na divisão quanto na subtração a ordem dos operandos devem ser mantidas
                                   | Number x , d -> S.Push(Number x); stackator d; 
                                   | d , Number y ->  S.Push (Number y); stackator d
                                   | v , k -> stackator (v); stackator (k)// stackator a; stackator b
    | Multiply (a, b) -> X.Push("Multiply"); match (a,b) with
                                    | Number x, Number y -> S.Push(Number x); S.Push(Number y)
                                    | Number x , d -> S.Push(Number x); stackator d; 
                                    | d , Number y ->  S.Push (Number y); stackator d
                                    | v , k -> stackator (v); stackator (k) //stackator a; stackator b
    | Divide (a, b) -> X.Push("Divide"); match (a,b) with
                                   | Number x, Number y ->  S.Push(Number y);S.Push(Number x);
                                   | Number x , d -> S.Push(Number x); stackator d; 
                                   | d , Number y ->  S.Push (Number y); stackator d
                                   | v , k -> stackator (v); stackator (k) //stackator a; stackator b
    | And (a, b) -> X.Push("And"); match a,b with
                                    | Boolean x, Boolean y -> S.Push(Boolean x); S.Push(Boolean y)
                                    | Boolean x , d -> S.Push(Boolean x); stackator d; 
                                    | d , Boolean y -> S.Push (Boolean y); stackator d
                                    | v , k -> stackator (v); stackator (k)
    | Or (a, b) -> X.Push("Or");  match a,b with
                                    | Boolean x, Boolean y -> S.Push(Boolean x); S.Push(Boolean y)
                                    | Boolean x , d -> S.Push(Boolean x); stackator d; 
                                    | d , Boolean y -> S.Push (Boolean y); stackator d
                                    | v , k -> stackator (v); stackator (k)
    | Neg a -> X.Push("Neg"); match a with 
                              | Boolean x -> S.Push(Boolean x)
                              | d -> stackator(d)
    //Comparaçõs booleanas
    //a ou b aqui só podem ser números ou o id de alguma variável: tem que separar em 2 (ou 4) casos distintos: se for numeros, ja resolve, se tiver um id, pega o valor na memória pra resovler
    | Eq (a,b) -> X.Push("Eq"); S.Push(b); S.Push(a) //match a,b with
                               //| Number a, Number b -> S.Push(Number b); S.Push(Number a); --> NOTA: esse casamento de padrão não parece se encaixar aqui. A idéia é resolver caso sejam IDs também (indo na memória e pegando o valor correspondente à
                               //id. O melhor jeito que eu vejo de fazer isso é com um dicionário. Essa operação pode ser feita no calculadora de Bool
                               //| Id a, Id b -> S.Push(Id b);
    | Leb (a,b) -> X.Push("Leb"); S.Push(b); S.Push(a);
    | Leq (a,b) -> X.Push("Leq"); S.Push(b); S.Push(a);
    | Geb (a,b) -> X.Push("Geb"); S.Push(b); S.Push(a);
    | Geq (a,b) -> X.Push("Geq"); S.Push(b); S.Push(a);
    | Neq (a,b) -> X.Push("Neq"); S.Push(b); S.Push(a);
    //comandos TODO Assign, While e If.
    | Assign (a,b) -> X.Push("Assign"); S.Push(Id a); stackator b
    | If (a,b,c) -> X.Push("If") ; S.Push(c); S.Push(b); S.Push(a) //IDEIA: segundo plotkin, empulha os comandos todos na pilha S. Se a for verdade, executar b e desempilhar c, se a não for verdade, desempilha b e executa c.
    | Loop (a,b) -> X.Push("Loop") ; S.Push(a); S.Push(b) //a semántica das regras de eliminação E1 e E2 do plotkin virão das calculadoras
    //real if e while não sei fazer direito não.

//let commandCalculator  (X: Stack<string>) (S: Stack<Cmd>) =
//    while X.Count <> 0 do
//       let op = X.Pop()
//        let d1 = S.Pop()
//        let d2 = S.Peek() //famosa gambiarra
//        match op with
//            | "Assign" -> match (d1,d2) with //queria reutilizar uma possivel calculadora, mas vamos ver:
//                         | Id a, k -> match k with //BUGADO: necessitamos de uma forma de resolver k antes de fazer M.Item(a,k), ou seja, atribuir k ao valor "a" na memória M.                                     
        

let getFromParser (exp) =
    match exp with
    | Success r -> printfn "Input = %A" (r.value); stackator r.value
    | Failure _ -> failwith "Parsing falhou!"