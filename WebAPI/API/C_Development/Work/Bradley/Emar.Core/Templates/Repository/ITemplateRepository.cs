using System;
using System.Collections.Generic;
using System.Text;
using Emar.Data.Entities;

namespace Emar.Core.Templates.Repository
{
    public interface ITemplateRepository
    {
        Template GetTemplate(int templateId);
    }
}
