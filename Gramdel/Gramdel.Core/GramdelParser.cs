using System.Collections.Generic;
using System.Text;

namespace Gramdel.Core
{
    public class GramdelParser
    {
        public void Gramdel(ParsingLocalContext context)
        {
            var prod = context.GetOrCreateProduction(this.Gramdel);

            var gramdelNode = new GramdelNode();

            // waiting for the first rule
            var prodRule = context.GetOrCreateProduction(this.Rule);
            SuccessContinuation<RuleNode> ruleProcessor = null;
            ruleProcessor = (ruleNode, ctx) =>
                {
                    // If there are no more rules, then we signal the production of a new Gramdel file.
                    if (ruleNode == null)
                    {
                        prod.ItemProduced(0, ctx, gramdelNode);
                    }
                    else
                    {
                        gramdelNode.Rules.Add(ruleNode);

                        // waiting for another rule
                        var prodRule2 = ctx.GetOrCreateProduction(this.Rule);
                        prodRule2.ContinueWith(ruleProcessor);
                        prodRule2.Execute(ctx);
                    }
                };
            prodRule.ContinueWith(ruleProcessor);
            prodRule.Execute(context);
        }

        private void Alternation(ParsingLocalContext context)
        {
            var prod = context.GetOrCreateProduction(this.Alternation);

            context.Position = prod.Origin;

            // Sets the number of alternatives that can be produced by this producer method.
            prod.SetAlternativeProductsCapacity(2);
            const int ALT_0 = 0;
            const int ALT_1 = 1;

            {
                var operands = new List<ExpressionNode>();
                var productionConcat = context.GetOrCreateProduction(this.Concatenation);
                FailureContinuation fail = ctx =>
                    {

                    };
                productionConcat.FailWith(fail);
                SuccessContinuation<ExpressionNode> cont1 = null;
                cont1 = (node, ctx) =>
                {
                    operands.Add(node);

                    ctx.Position += ctx.Reader.SkipSpaces(ctx.Code, ctx.Position);
                    if (ctx.Reader.TryReadToken(ctx.Code, ctx.Position, "|"))
                    {
                        ctx.Position++;
                        ctx.Position += ctx.Reader.SkipSpaces(ctx.Code, ctx.Position);

                        // reading the second operator of the alternation
                        var productionConcat2 = context.GetOrCreateProduction(this.Concatenation);
                        productionConcat2.ContinueWith(cont1);
                        productionConcat2.Execute(ctx);
                    }
                    else if (operands.Count > 1)
                    {
                        prod.ItemProduced<ExpressionNode>(ALT_0, ctx, new AlternationNode { Operands = operands.ToArray() });
                        prod.ProductionFailed(ALT_1, ctx);
                    }
                    else if (operands.Count == 1)
                    {
                        prod.ProductionFailed(ALT_0, ctx);
                        prod.ItemProduced(ALT_1, ctx, node);
                    }
                    else
                    {
                        prod.ProductionFailed(ALT_0, ctx);
                        prod.ProductionFailed(ALT_1, ctx);
                    }
                };
                productionConcat.ContinueWith(cont1);
                productionConcat.Execute(context);
            }
        }

        private void Concatenation(ParsingLocalContext context)
        {
            var prod = context.GetOrCreateProduction(this.Concatenation);

            var origin = context.Position;

            context.Position = origin;

            // Sets the number of alternatives that can be produced by this producer method.
            prod.SetAlternativeProductsCapacity(2);
            const int ALT_0 = 0;
            const int ALT_1 = 1;

            {
                var operands = new List<ExpressionNode>();
                var prodExpr1 = context.GetOrCreateProduction(this.ExpressionTerm);
                SuccessContinuation<ExpressionNode> cont1 = null;
                cont1 = (node, ctx) =>
                {
                    operands.Add(node);

                    var spaces = ctx.Reader.SkipSpaces(ctx.Code, ctx.Position);
                    if (spaces > 0)
                    {
                        ctx.Position += spaces;

                        // reading the second operator of the alternation
                        var prodExpr2 = ctx.GetOrCreateProduction(this.ExpressionTerm);
                        prodExpr2.ContinueWith(cont1);
                        prodExpr2.Execute(ctx);
                    }
                    else if (operands.Count > 1)
                    {
                        prod.ItemProduced<ExpressionNode>(ALT_0, ctx, new ConcatenationNode { Operands = operands.ToArray() });
                        prod.ProductionFailed(ALT_1, ctx);
                    }
                    else if (operands.Count == 1)
                    {
                        prod.ProductionFailed(ALT_0, ctx);
                        prod.ItemProduced(ALT_1, ctx, node);
                    }
                    else
                    {
                        prod.ProductionFailed(ALT_0, ctx);
                        prod.ProductionFailed(ALT_1, ctx);
                    }
                };
                prodExpr1.ContinueWith(cont1);
                prodExpr1.Execute(context);
            }
        }

        private void ExpressionTerm(ParsingLocalContext context)
        {
            var prod = context.GetOrCreateProduction(this.ExpressionTerm);

            prod.SetAlternativeProductsCapacity(2);
            const int ALT_TOKEN = 0;
            const int ALT_NAME = 1;

            bool isAltTokenFailed = false;

            // trying to read a token
            {
                context.Position = prod.Origin;

                if (context.Reader.TryReadToken(context.Code, context.Position, "'"))
                {
                    context.Position++;

                    var b = new StringBuilder();
                    var tokenData = context.Reader.ReadText(context.Code, context.Position, c => c != '\'');
                    context.Position += tokenData.Length;
                    b.Append(tokenData);
                    while (context.Reader.TryReadToken(context.Code, context.Position, "''"))
                    {
                        context.Position += 2;
                        b.Append('\'');
                        tokenData = context.Reader.ReadText(context.Code, context.Position, c => c != '\'');
                        context.Position += tokenData.Length;
                        b.Append(tokenData);
                    }

                    if (context.Reader.TryReadToken(context.Code, context.Position, "'"))
                    {
                        context.Position++;
                        prod.ItemProduced<ExpressionNode>(ALT_TOKEN, context, new TokenNode { Token = b.ToString() });
                    }
                    else
                        isAltTokenFailed = true;
                }
                else
                    isAltTokenFailed = true;

                if (isAltTokenFailed)
                    prod.ProductionFailed(ALT_TOKEN, context);
            }

            // trying to read a name
            {
                context.Position = prod.Origin;

                var name = context.Reader.ReadText(context.Code, context.Position, c => char.IsLetterOrDigit(c));
                if (name.Length > 0)
                {
                    context.Position += name.Length;
                    prod.ItemProduced<ExpressionNode>(ALT_NAME, context, new RuleReferenceNode { RuleName = name });
                }
                else
                {
                    prod.ProductionFailed(ALT_NAME, context);
                }
            }
        }

        public void Rule(ParsingLocalContext context)
        {
            var prod = context.GetOrCreateProduction(this.Rule);

            var ruleNode = new RuleNode();

            context.Position += context.Reader.SkipSpaces(context.Code, context.Position);
            if (context.Reader.TryReadToken(context.Code, context.Position, "rule"))
            {
                context.Position += 4;
                var spaces = context.Reader.SkipSpaces(context.Code, context.Position);

                if (spaces > 0)
                {
                    context.Position += spaces;
                    ruleNode.Name = context.Reader.ReadText(context.Code, context.Position, c => char.IsLetterOrDigit(c));
                    context.Position += ruleNode.Name.Length;

                    context.Position += context.Reader.SkipSpaces(context.Code, context.Position);

                    if (context.Reader.TryReadToken(context.Code, context.Position, "->"))
                    {
                        context.Position += 2;
                        context.Position += context.Reader.SkipSpaces(context.Code, context.Position);

                        var prodAlt = context.GetOrCreateProduction(this.Alternation);
                        prodAlt.ContinueWith<ExpressionNode>((expressionNode, ctx) =>
                            {
                                ctx.Position += ctx.Reader.SkipSpaces(ctx.Code, ctx.Position);

                                if (ctx.Reader.TryReadToken(ctx.Code, ctx.Position, ";"))
                                {
                                    ctx.Position++;
                                    ruleNode.Expression = expressionNode;
                                    prod.ItemProduced(0, context, ruleNode);
                                }
                            });
                        prodAlt.Execute(context);
                    }
                }
            }
        }

        public class GramdelNode
        {
            public GramdelNode()
            {
                this.Rules = new List<RuleNode>();
            }

            public List<RuleNode> Rules { get; set; }
        }

        public class RuleNode
        {
            public string Name { get; set; }
            public ExpressionNode Expression { get; set; }
        }

        public abstract class ExpressionNode
        {
        }

        public class ConcatenationNode : ExpressionNode
        {
            public ExpressionNode[] Operands { get; set; }
        }

        public class AlternationNode : ExpressionNode
        {
            public ExpressionNode[] Operands { get; set; }
        }

        public class RuleReferenceNode : ExpressionNode
        {
            public string RuleName { get; set; }
        }

        public class TokenNode : ExpressionNode
        {
            public string Token { get; set; }
        }

    }
}
