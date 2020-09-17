using System.Collections.Generic;
using System.Linq;
using Emar.Data;
using Emar.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Emar.Core.Templates.Repository
{
    public class TemplateRepository : ITemplateRepository
    {
        private readonly EmarContext _context;

        public TemplateRepository(EmarContext context)
        {
            _context = context;
        }

        public Template GetTemplate(int templateId)
        {
            return _context.Templates
                .Include(t => t.TemplatePromptGroups)
                .ThenInclude(tp => tp.PromptGroup)
                .ThenInclude(pg => pg.Prompts)
                .ThenInclude(p => p.PromptChoices)
                .FirstOrDefault(t => t.Id == templateId);
        }
    }
}